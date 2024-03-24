using System;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using Peasmod4.API.Networking;
using Peasmod4.API.Roles;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Peasmod4.API.UI.Options;

[HarmonyPatch]
public class Patches
{
    const float OPTION_SIZE = 0.5f; 
    const float FIRST_OPTION_Y = 2.1f;
    
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    [HarmonyPostfix]
    public static void CreateOptionsPatch(GameOptionsMenu __instance)
    {
        PeasmodPlugin.Logger.LogInfo("1");
        CustomOptionManager.NumberOptionPrefab = Object.FindObjectsOfType<NumberOption>().FirstOrDefault();
            
        CustomOptionManager.ToggleOptionPrefab = Object.FindObjectOfType<ToggleOption>(); 
            
        CustomOptionManager.StringOptionPrefab = Object.FindObjectsOfType<StringOption>().FirstOrDefault();

        PeasmodPlugin.Logger.LogInfo("2: " + CustomOptionManager.MenuVisibleOptions.Count);
        if (CustomOptionManager.MenuVisibleOptions.Count == 0)
            return;
        
        foreach (var customOption in CustomOptionManager.MenuVisibleOptions.FindAll(option => option.AdvancedVanillaOption))
        {
            OptionBehaviour option = customOption.CreateOption();

            option.transform.localPosition = new Vector3(option.transform.localPosition.x, FIRST_OPTION_Y -
                (__instance.Children.Count) * OPTION_SIZE, -1f);
        
            var options = __instance.Children.ToList();
            options.Add(option);
            __instance.Children = options.ToArray();
        }
        
        foreach (var customOption in CustomOptionManager.MenuVisibleOptions.FindAll(option => !option.AdvancedVanillaOption))
        {
            OptionBehaviour option = customOption.CreateOption();

            option.transform.localPosition = new Vector3(option.transform.localPosition.x, FIRST_OPTION_Y -
                (__instance.Children.Count + 1) * OPTION_SIZE, -1f);
        
            var options = __instance.Children.ToList();
            options.Add(option);
            __instance.Children = options.ToArray();
        }

        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = (__instance.Children.Count + 1) * OPTION_SIZE - 8.6f * OPTION_SIZE;
    }

    private static System.Collections.Generic.List<Assembly> _assemblies;
    private static System.Collections.Generic.List<Assembly> GetAssemblies()
    {
        if (_assemblies != null)
            return _assemblies;

        _assemblies = new System.Collections.Generic.List<Assembly>();
        /*foreach (var assembly in CustomRoleManager.Roles.ConvertAll(role => role.Assembly))
        {
            if (!_assemblies.Contains(assembly))
                _assemblies.Add(assembly);
        }*/
        //_assemblies.AddRange(CustomRoleManager.Roles.ConvertAll(role => role.Assembly).FindAll(assembly => !_assemblies.Contains(assembly)));
        foreach (var assembly in CustomOptionManager.CustomOptions.FindAll(option => !option.AdvancedVanillaOption).ConvertAll(option => option.Assembly))
        {
            if (!_assemblies.Contains(assembly))
                _assemblies.Add(assembly);
        }
        //_assemblies.AddRange(CustomOptionManager.CustomOptions.ConvertAll(option => option.Assembly).FindAll(assembly => !_assemblies.Contains(assembly)));
        return _assemblies;
    }
    
    private static int PageIndex;
    
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    [HarmonyPostfix]
    private static void SwitchSettingsPagesPatch(KeyboardJoystick __instance)
    {
        if (GetAssemblies().Count == 0)
            return;
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PageIndex == GetAssemblies().Count)
                PageIndex = 0;
            else
                PageIndex++;
        }
    }

    public const string SwitchPagesInstruction = "Press <b>TAB</b> to browse through the settings";
    
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    [HarmonyPrefix]
    private static bool RenderCustomOptionsPatch([HarmonyArgument(0)] IGameOptions gameOptions, ref string __result)
    {
        if (PageIndex == 0)
            return true;
        
        var builder = IGameOptionsExtensions.SettingsStringBuilder;
        if (CustomOptionManager.CustomRoleOptions.Count + CustomOptionManager.HudVisibleOptions.Count <= 0)
            return true;

        var assembly = GetAssemblies()[PageIndex - 1];
        
        builder.Clear();
        builder.AppendLine(SwitchPagesInstruction);
        builder.AppendLine($"Page {PageIndex} / {GetAssemblies().Count}: <b>{assembly.GetName().Name}</b>");
        builder.AppendLine();
        
        var assemblyOptions = CustomOptionManager.CustomOptions.FindAll(option => option.Assembly == assembly && !option.AdvancedVanillaOption && !option.AdvancedRoleOption);
        var assemblyRoleOptions = CustomOptionManager.CustomRoleOptions.FindAll(option => option.Role.Assembly == assembly);
        if (assemblyOptions.Count > 0)
        {
            assemblyOptions.Do(option => option.RenderOption(builder));
            if (assemblyRoleOptions.Count > 0)
                builder.AppendLine();
        }

        if (assemblyRoleOptions.Count > 0)
        {
            builder.AppendLine("<b>Roles:</b>");
            assemblyRoleOptions.Do(option =>
            {
                builder.AppendLine(
                    option.Role.Color.ToTextColor() + DestroyableSingleton<TranslationController>.Instance.GetString(option.Role.RoleBehaviour.StringName) + Utility.StringColor.Reset
                    + ": " + DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.RoleChanceAndQuantity, new Il2CppSystem.Object[]
                        {
                            option.Count,
                            option.Chance
                        }));
                var associatedAdvancedOptions = option.AdvancedOptions;
                if (associatedAdvancedOptions.Length > 0)
                {
                    associatedAdvancedOptions.Do(option => option.RenderOption(builder, " - "));
                }
            });
        }

        __result = builder.ToString();

        return false;
    }

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
    [HarmonyPostfix]
    private static void RenderVanillaOptionsPatch([HarmonyArgument(0)] IGameOptions gameOptions, ref string __result)
    {
        if (PageIndex != 0)
            return;

        var builder = IGameOptionsExtensions.SettingsStringBuilder;
        var text = builder.ToString().Trim();
        builder.Clear();
        if (GetAssemblies().Count > 0)
        {
            builder.AppendLine(SwitchPagesInstruction);
            builder.AppendLine($"Page 0 / {GetAssemblies().Count}: <b>Vanilla</b>");
            builder.AppendLine();
        }
        builder.AppendLine(text);
        
        foreach (var role in CustomRoleManager.Roles)
        {
            var roleBehaviour = role.RoleBehaviour;
            builder.Replace(
                "\n" + DestroyableSingleton<TranslationController>.Instance.GetString(roleBehaviour.StringName) +
                ": " + DestroyableSingleton<TranslationController>.Instance.GetString(
                    StringNames.RoleChanceAndQuantity, new Il2CppSystem.Object[]
                    {
                        gameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role),
                        gameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role)
                    }), "");
        }
        
        foreach (var option in CustomOptionManager.CustomOptions.FindAll(option => option.AdvancedVanillaOption))
        {
            option.RenderOption(builder);
        }
        
        __result = builder.ToString();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    [HarmonyPostfix]
    private static void AmongUsClientOnPlayerJoinedPatch(AmongUsClient __instance,
        [HarmonyArgument(0)] ClientData client)
    {
        if (__instance.AmHost)
        {
            foreach (var option in CustomOptionManager.CustomOptions)
            {
                switch (option)
                {
                    case CustomToggleOption customOption:
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, customOption.Value));
                        break;
                    case CustomNumberOption customOption:
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, customOption.Value));
                        break;
                    case CustomStringOption customOption:
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, customOption.Value));
                        break;
                    case CustomRoleOption customOption:
                        Rpc<RpcUpdateSetting>.Instance.SendTo(client.Id, new RpcUpdateSetting.Data(option, customOption.Count, customOption.Chance));
                        break;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.OnEnable))]
    [HarmonyPostfix]
    public static void CreateRoleOptionsPatch(RolesSettingsMenu __instance)
    {
        var roleSettingPrefab = CustomOptionManager.RoleOptionPrefab = __instance.AllRoleSettings.ToArray()[0];
        CustomOptionManager.AdvancedRoleSettingsPrefab = __instance.AllAdvancedSettingTabs.ToArray()[0].Tab;
        foreach (var option in CustomOptionManager.CustomRoleOptions)
        {
            if (option.GetType() == typeof(CustomRoleOption))
            {
                var newSetting = option.CreateOption();
                newSetting.transform.localPosition = roleSettingPrefab.transform.localPosition - 
                    new Vector3(0f , (__instance.AllRoleSettings.ToArray().Count + CustomOptionManager.CustomRoleOptions.IndexOf(option) + 1 + GetAssemblies().IndexOf(option.Assembly)) * 0.5f);

                if (!option.AdjustRoleSettings)
                {
                    var roleOptionObject = (RoleOptionSetting)newSetting;
                    //roleOptionObject.ChanceText.gameObject.SetActive(false);
                    //roleOptionObject.CountText.gameObject.SetActive(false);
                    roleOptionObject.transform.FindChild("Count Plus_TMP").gameObject.SetActive(false);
                    roleOptionObject.transform.FindChild("Count Minus_TMP").gameObject.SetActive(false);
                    roleOptionObject.transform.FindChild("Chance Plus_TMP").gameObject.SetActive(false);
                    roleOptionObject.transform.FindChild("Chance Minus_TMP").gameObject.SetActive(false);
                }
                
                if (__instance.AllAdvancedSettingTabs.WrapToSystem()
                    .Find(button => button.Type == option.Role.RoleBehaviour.Role) == null)
                {
                    var tab = option.CreateAdvancedOptions();
                    if (tab != null)
                        __instance.AllAdvancedSettingTabs.Add(tab);
                }
                
                option.UpdateValuesAndText();
            }
        }

        var scroller = roleSettingPrefab.gameObject.transform.parent.parent.GetComponent<Scroller>();
        scroller.OnScrollYEvent += (Action<float>) (value =>
        {
            for (int i = 0; i < scroller.Inner.childCount; i++)
            {
                var child = scroller.Inner.GetChild(i);
                child.gameObject.SetActive(child.position.y < GameObject.Find("RolesChancesGroup/Text").transform.position.y);
            }
        });
        scroller.ContentYBounds.max =
            (CustomOptionManager.CustomRoleOptions.Count + GetAssemblies().Count - 4.5f) * 0.5f;
        scroller.transform.FindChild("UI_Scrollbar").gameObject.SetActive(true);
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.ValueChanged))]
    [HarmonyPrefix]
    public static bool RoleOptionRoleRateChangePatch(RolesSettingsMenu __instance,
        [HarmonyArgument(0)] OptionBehaviour optionBehaviour)
    {
        var customOption = optionBehaviour.GetCustomOption();
        PeasmodPlugin.Logger.LogInfo(customOption);
        
        if (customOption == null || customOption is not CustomRoleOption)
            return true;

        var roleOption = optionBehaviour as RoleOptionSetting;
        var customRoleOption = customOption as CustomRoleOption;
        customRoleOption.SetValue(roleOption.RoleMaxCount, roleOption.RoleChance);
        customRoleOption.UpdateValuesAndText();
        PeasmodPlugin.Logger.LogInfo("Data has changed: " + roleOption.RoleMaxCount + " " + roleOption.RoleChance);
        
        return false;
    }

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.ValueChanged))]
    [HarmonyPostfix]
    public static void RoleOptionValueChangedPatch(RolesSettingsMenu __instance,
        [HarmonyArgument(0)] OptionBehaviour optionBehaviour)
    {
        PeasmodPlugin.Logger.LogInfo("Test " + optionBehaviour.IsCustomOption());

        if (optionBehaviour.IsCustomOption())
        {
            var custom = optionBehaviour.GetCustomOption();
            switch (custom)
            {
                /*case CustomRoleOption option:
                    RoleOptionSetting roleOptionSetting = optionBehaviour as RoleOptionSetting;
                    PeasmodPlugin.Logger.LogInfo("Test3 " + roleOptionSetting.RoleMaxCount + " " + roleOptionSetting.RoleChance);
                    break;*/
                case CustomToggleOption option:
                    option.SetValue(((ToggleOption) optionBehaviour).GetBool());
                    break;
                case CustomNumberOption option:
                    option.SetValue(((NumberOption) optionBehaviour).GetFloat());
                    break;
                case CustomStringOption option:
                    option.SetValue(((StringOption) optionBehaviour).GetInt());
                    break;
            }
        }
    }
    
    private static float HudTextSize = 1.4f;
    private static Scroller OptionsScroller;
    
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    private static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (__instance.GameSettings == null)
            return;
            
        __instance.GameSettings.fontSizeMin =
            __instance.GameSettings.fontSizeMax = 
                __instance.GameSettings.fontSize = HudTextSize;
            
        CreateScroller(__instance);
        
        var bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) - Camera.main.transform.localPosition;

        OptionsScroller.SetYBoundsMin(-bottomLeft.y - 0.1f);
        OptionsScroller.SetYBoundsMax(Mathf.Max(-bottomLeft.y - 0.1f, __instance.GameSettings.renderedHeight - -bottomLeft.y + 0.1f));
    }

    //THIS BIT IS SKIDDED FROM ESSENTIALS: https://github.com/DorCoMaNdO/Reactor-Essentials
    private static void CreateScroller(HudManager hudManager)
    {
        if (OptionsScroller != null) return;

        OptionsScroller = new GameObject("OptionsScroller").AddComponent<Scroller>();
        OptionsScroller.transform.SetParent(hudManager.GameSettings.transform.parent);
        OptionsScroller.gameObject.layer = 5;

        OptionsScroller.transform.localScale = Vector3.one;
        OptionsScroller.allowX = false;
        OptionsScroller.allowY = true;
        OptionsScroller.active = true;
        OptionsScroller.velocity = new Vector2(0, 0);
        OptionsScroller.ContentYBounds = new FloatRange(0, 0);
        OptionsScroller.enabled = true;

        OptionsScroller.Inner = hudManager.GameSettings.transform;
        hudManager.GameSettings.transform.SetParent(OptionsScroller.transform);
    }
}