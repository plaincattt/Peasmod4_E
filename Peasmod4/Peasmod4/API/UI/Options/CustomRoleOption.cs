using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Peasmod4.API.Networking;
using Peasmod4.API.Roles;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Peasmod4.API.UI.Options;

public class CustomRoleOption : CustomOption
{
    public CustomRoleOption(CustomRole role, bool adjustRoleSettings, params CustomOption[] advancedOptions) : base(role.Name)
    {
        Assembly = System.Reflection.Assembly.GetCallingAssembly();
        Id = $"{Assembly.GetName().Name}.RoleOption.{role.Id}";
        
        Role = role;
        AdjustRoleSettings = adjustRoleSettings;
        HudFormat = "{0}: {1} with {2}% Chance";
        foreach (var advancedOption in advancedOptions)
        {
            advancedOption.AdvancedRoleOption = true;
            //advancedOption.Id += $".AdvancedRoleOption.{role.Name}";
        }
        AdvancedOptions = advancedOptions;
        
        try
        {
            _countConfigEntry = PeasmodPlugin.ConfigFile.Bind("Options", role.Name + ".Count", 0);
        }
        catch (Exception e)
        {
            PeasmodPlugin.Logger.LogError($"Error while loading the option \"{role.Name + ".Count"}\": {e.Message}");
        }
        Count = _countConfigEntry?.Value ?? 0;
        if (AdjustRoleSettings)
            Role.Count = Count;
        else
            Count = Role.Count;
        
        try
        {
            _chanceConfigEntry = PeasmodPlugin.ConfigFile.Bind("Options", role.Name + ".Chance", 0);
        }
        catch (Exception e)
        {
            PeasmodPlugin.Logger.LogError($"Error while loading the option \"{role.Name + ".Chance"}\": {e.Message}");
        }
        Chance = _chanceConfigEntry?.Value ?? 0;
        if (AdjustRoleSettings)
            Role.Chance = Chance;
        else
            Chance = Role.Chance;
        
        CustomOptionManager.CustomRoleOptions.Add(this);
    }

    public CustomRole Role;
    public bool AdjustRoleSettings;
    public int Count;
    public int Chance;
    public CustomOption[] AdvancedOptions;

    private ConfigEntry<int> _countConfigEntry;
    private ConfigEntry<int> _chanceConfigEntry;
    
    public delegate void OnValueChangedHandler(CustomRoleOptionValueChangedArgs args);
    public event OnValueChangedHandler OnValueChanged;
    public class CustomRoleOptionValueChangedArgs
    {
        public CustomRoleOption Option;
        public int OldCount;
        public int NewCount;
        public int OldChance;
        public int NewChance;
            
        public CustomRoleOptionValueChangedArgs(CustomRoleOption option, int oldCount, int newCount, int oldChance, int newChance)
        {
            Option = option;
            OldCount = oldCount;
            NewCount = newCount;
            OldChance = oldChance;
            NewChance = newChance;
        }
    }
    
    public RoleOptionSetting RoleOption => (RoleOptionSetting) Option;
    
    public override OptionBehaviour CreateOption()
    {
        if (Option != null)
        {
            return Option;
        }
        
        var newSetting = Object.Instantiate(CustomOptionManager.RoleOptionPrefab, CustomOptionManager.RoleOptionPrefab.transform.parent);
        newSetting.name = Role.Name + "-RoleOption";
        newSetting.Role = Role.RoleBehaviour;
        newSetting.TitleText.text = Role.Name;
        newSetting.TitleText.color = Role.Color;
        newSetting.RoleMaxCount = Count;
        newSetting.RoleChance = Chance;
        /*newSetting.SetRole(GameOptionsManager.Instance.currentGameOptions.RoleOptions);
        newSetting.OnValueChanged = new Action<OptionBehaviour>(option =>
        {
            var roleOption = option as RoleOptionSetting;
            PeasmodPlugin.Logger.LogInfo("Data has changed: " + roleOption.RoleMaxCount + " " + roleOption.RoleChance);
            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.SetRoleRate(roleOption.Role.Role, roleOption.RoleMaxCount, roleOption.RoleChance);
            roleOption.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
        });*/
        newSetting.transform.FindChild("More Options").gameObject.SetActive(false);
        
        /*var roleData = new RoleDataV07(Role.RoleBehaviour.Role);
        GameOptionsManager.Instance.currentNormalGameOptions.roleOptions.AddOrUpdateRole(roleData);
        
        Count = GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(Role.RoleBehaviour.Role);
        Chance = GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetChancePerGame(Role.RoleBehaviour.Role);*/
            
        Option = newSetting;
        
        return newSetting;
    }

    public AdvancedRoleSettingsButton CreateAdvancedOptions()
    {
        if (AdvancedOptions.Length == 0)
            return null;
        
        Option.transform.FindChild("More Options").gameObject.SetActive(true);
        
        var tab = Object.Instantiate(CustomOptionManager.AdvancedRoleSettingsPrefab, CustomOptionManager.AdvancedRoleSettingsPrefab.transform.parent);
        tab.name = Role.Name + " Settings";
        
        foreach (var option in tab.GetComponentsInChildren<OptionBehaviour>())
        {
            option.gameObject.DestroyImmediate();
        }

        var scrollerObj = new GameObject("Scroller");
        scrollerObj.transform.parent = tab.transform;
        scrollerObj.layer = LayerMask.NameToLayer("UI");
        scrollerObj.transform.localPosition = new Vector3(0f, 0f);
        var scroller = scrollerObj.AddComponent<Scroller>();
        scroller.allowX = false;
        scroller.allowY = true;
        scroller.transform.localScale = Vector3.one;
        scroller.active = true;
        scroller.velocity = new Vector2(0, 0);
        scroller.ContentYBounds = new FloatRange(0, (3.54f - AdvancedOptions.Length * 0.56f) * (-1f));
        scroller.enabled = true;

        var inner = new GameObject("Inner");
        inner.transform.parent = scrollerObj.transform;
        inner.transform.localPosition = new Vector3(0f, 0f);
        scroller.Inner = inner.transform;

        /*var boxCollider = new GameObject("Mask");
        boxCollider.transform.parent = tab.transform;
        var col = boxCollider.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);*/

        var collider = Object.Instantiate(tab.transform.parent.parent.FindChild("Background").FindChild("MaskArea"), tab.transform);
        collider.name = "Collider";
        collider.GetComponent<SpriteMask>().Destroy();
        collider.gameObject.SetActive(false);
        collider.localPosition = new Vector3(0f, 0f);
        
        //scroller.Hitbox = boxCollider;
        var list = new List<Collider2D>();
        list.Add(collider.GetComponent<BoxCollider2D>());
        scroller.Colliders = list.ToArray();
        
        foreach (var advancedOption in AdvancedOptions)
        {
            var option = advancedOption.CreateOption();
            var optionTransform = option.transform;
            optionTransform.parent = inner.transform;
            //optionTransform.localScale = Vector3.one;
            optionTransform.localPosition =
                new Vector3(-1.25f, 0.06f - AdvancedOptions.ToList().IndexOf(advancedOption) * 0.56f, 0f);
        }
            
        var roleName = tab.transform.FindChild("Role Name");
        roleName.GetComponent<TextTranslatorTMP>().Destroy();
        roleName.GetComponent<TextMeshPro>().text = Role.Name;
        
        return new AdvancedRoleSettingsButton
        {
            Tab = tab,
            Type = Role.RoleBehaviour.Role
        };
    }

    public void SetValue(int maxCount, int chance)
    {
        OnValueChanged?.Invoke(new CustomRoleOptionValueChangedArgs(this, Count, maxCount, Chance, chance));
        
        Count = maxCount;
        Chance = chance;

        if (_countConfigEntry != null && AmongUsClient.Instance.AmHost)
            _countConfigEntry.Value = Count;
        if (_chanceConfigEntry != null && AmongUsClient.Instance.AmHost)
            _chanceConfigEntry.Value = Chance;

        if (AmongUsClient.Instance.AmHost)
        {
            RoleOption.RoleMaxCount = maxCount;
            RoleOption.RoleChance = chance;
            Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, maxCount, chance));
        }
        
        if (AdjustRoleSettings)
        {
            Role.Count = Count;
            Role.Chance = Chance;
        }
    }

    public void UpdateValuesAndText()
    {
        RoleOption.CountText.text = Count.ToString();
        RoleOption.ChanceText.text = Chance + "%";
    }
}