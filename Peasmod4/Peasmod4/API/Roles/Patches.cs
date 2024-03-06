using System;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Networking;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using UnityEngine;
using EventType = Peasmod4.API.Events.EventType;
using Random = System.Random;

namespace Peasmod4.API.Roles;

[HarmonyPatch]
public class Patches
{
    /*
     * General patches
     */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(FreeWeekendShower), nameof(FreeWeekendShower.Start))]
    public static void InjectRoleBehavioursPatch()
    {
        CustomRoleManager.InjectRoleBehaviours();
    }
    
    /*
     * Role patches
     */
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static void AssignRolesPatch(RoleManager __instance)
    {
        System.Collections.Generic.List<GameData.PlayerInfo> players =
            GameData.Instance.AllPlayers.WrapToSystem().FindAll(playerInfo => !playerInfo.Disconnected && !playerInfo.IsDead && playerInfo.Object != null && !playerInfo.Object.isDummy);//new System.Collections.Generic.List<GameData.PlayerInfo>();

        var crewmates = players.FindAll(player => !player.Role.IsImpostor && player.Role.IsSimpleRole);
        foreach (var playerInfo in crewmates)
        {
            PeasmodPlugin.Logger.LogInfo("Crewmate: " + playerInfo.Object.name);
        }
        var impostors = players.FindAll(player => player.Role.IsImpostor && player.Role.IsSimpleRole);
        foreach (var playerInfo in impostors)
        {
            PeasmodPlugin.Logger.LogInfo("Impostor: " + playerInfo.Object.name);
        }
        
        var roles = CustomRoleManager.Roles;
        roles.Sort((role1, role2) =>
        {
            var difference = role2.Chance - role1.Chance;
            if (difference == 0)
                difference = Random.Shared.Next(-1, 2);
            return difference;
        });
        foreach (var role in roles)
        {
            for (int i = 0; i < role.Count; i++)
            {
                if (HashRandom.Next(101) >= role.Chance)
                    continue;
                
                var isImpostor = role.Team == Enums.Team.Impostor;
                
                if (isImpostor && impostors.Count == 0)
                    continue;
                
                if (!isImpostor && crewmates.Count == 0)
                    continue;
                
                var player = isImpostor ? impostors[HashRandom.FastNext(impostors.Count)] : crewmates[HashRandom.FastNext(crewmates.Count)];
                crewmates.Remove(player);
                impostors.Remove(player);
                player.RpcSetCustomRole(role);

                PeasmodPlugin.Logger.LogInfo($"2: {player.PlayerName} was assigned {role.Name} role");
            }
        }
            
        //HideButtons(HudManager.Instance, true);
        Rpc<RpcTriggerEvent>.Instance.Send(new RpcTriggerEvent.Data("Start"));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    public static void AssignedRolePatch(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] RoleTypes role)
    {
        var customRole = CustomRoleManager.GetRole(role);
        if (customRole != null)
        {
            ToggleButtons(null, new HudEventManager.HudSetActiveEventArgs(HudManager.Instance, true));
            customRole.OnRoleAssigned(player);
        }
        
        foreach (var playerControl in PlayerControl.AllPlayerControls.WrapToSystem())
        {
            PlayerNameColor.Set(playerControl);
        }
    }
    
    /*[HarmonyPostfix]
    [HarmonyPatch(typeof(IntroCutscene._ShowTeam_d__36), nameof(IntroCutscene._ShowTeam_d__36.MoveNext))]
    public static void IntroShowTeamColorPatch(IntroCutscene._ShowTeam_d__36 __instance)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            var role = PlayerControl.LocalPlayer.GetCustomRole();
            __instance.__4__this.overlayHandle.color = role.Color;
            __instance.__4__this.BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
        }
    }*/
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    public static void Postfix(IntroCutscene __instance)
    {
        var role = PlayerControl.LocalPlayer.GetCustomRole();
        if (role != null)
        {
            __instance.BackgroundBar.material.SetColor("_Color", role.Color);
            __instance.TeamTitle.color = role.Color;
            __instance.TeamTitle.text = role.TeamText;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SelectTeamToShow))]
    public static bool ShowTeamMembersPatch(IntroCutscene __instance, ref List<PlayerControl> __result)
    {
        var role = PlayerControl.LocalPlayer.GetCustomRole();
        if (role != null)
        {
            bool filter(GameData.PlayerInfo info)
            {
                switch (role.Team)
                {
                    case Enums.Team.Alone:
                        return info.IsLocal();
                    case Enums.Team.Role:
                        return info.IsCustomRole(role);
                    case Enums.Team.Crewmate:
                        return true;
                    case Enums.Team.Impostor:
                        return info.Role.TeamType == RoleTeamTypes.Impostor;
                    default:
                        return false;
                }
            }

            __result = (from pcd in GameData.Instance.AllPlayers.WrapToSystem()
                where !pcd.Disconnected && filter(pcd)
                select pcd.Object).OrderBy(player => !player.IsLocal() ? 1 : 0).ToList().WrapToIl2Cpp();
            return false;
        }

        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
    public static void IntroShowRoleColorPatch(IntroCutscene._ShowRole_d__41 __instance)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            var role = PlayerControl.LocalPlayer.GetCustomRole();
            __instance.__4__this.RoleText.color = __instance.__4__this.RoleBlurbText.color = role.Color;
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerNameColor), nameof(PlayerNameColor.Set), typeof(PlayerControl))]
    public static void ShowRoleNamePatch([HarmonyArgument(0)] PlayerControl player)
    {
        if (!player.IsVisibleTo(PlayerControl.LocalPlayer))
        {
            player.cosmetics.SetNameColor(Color.white);
            player.cosmetics.nameText.text = player.name;
        }
        else
            player.SetNameFromRole();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    public static void ShowRolesCorrectlyInMeetingPatch1(MeetingHud __instance)
    {
        foreach (var voteArea in __instance.playerStates)
        {
            var player = voteArea.TargetPlayerId.GetPlayer();
            if (!player.IsVisibleTo(PlayerControl.LocalPlayer))
                voteArea.NameText.color = Color.white;
            else
                voteArea.SetNameFromRole(player);
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
    public static void ShowRolesCorrectlyInMeetingPatch2(PlayerVoteArea __instance, [HarmonyArgument(0)] GameData.PlayerInfo playerInfo)
    {
        var player = playerInfo.Object;
        if (!player.IsVisibleTo(PlayerControl.LocalPlayer))
        {
            __instance.NameText.text = player.name;
            __instance.NameText.color = Color.white;
        }
        else
        {
            __instance.SetNameFromRole(player);
        }
    }
    
    /*[HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void AddTaskHintPatch(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            if (__instance.taskDirtyTimer != 0f)
                return;

            var role = PlayerControl.LocalPlayer.GetCustomRole();
            
            __instance.tasksString.AppendFormat("\n\n<color=#{0}>{1} {2}</color>\n{3}", role.Color.ToHtmlStringRGBA(), role.Name,
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleHint), role.TaskHint);
            __instance.tasksString.TrimEnd();
            __instance.TaskPanel.SetTaskText(__instance.tasksString.ToString());
        }
    }*/

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public static void ConfirmRoleAfterEject(ExileController __instance)
    {
        if (__instance.exiled == null || !__instance.exiled.IsCustomRole() || !GameManager.Instance.LogicOptions.GetConfirmImpostor())
            return;

        var role = __instance.exiled.GetCustomRole();
        __instance.completeString =
            $"{__instance.exiled.PlayerName} was {(role.GetMembers().Count == 1 ? "The" : "A")} {role.Name} ({role.TeamText}).";
    }

    [RegisterEventListener(EventType.GameStart)]
    public static void SetButtonLabelMaterial(object sender, EventArgs args)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            PlayerControl.LocalPlayer.Data.Role.buttonManager.buttonLabelText.fontSharedMaterial =
                HudManager.Instance.UseButton.buttonLabelText.fontSharedMaterial;
        }
    }
    
    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.CanUse))]
    public static bool Test(RoleBehaviour __instance, ref bool __result)
    {
        // NOTE: Role Assignment broken caused by rates, ability button problem

        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            PeasmodPlugin.Logger.LogInfo("188gggalolololo");
            __result = true;
            return false;
        }
        
        return true;
    }*/
    
    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(ModRole), nameof(ModRole.CanUse))]
    public static bool CanUsePatch(ModRole __instance, [HarmonyArgument(0)] IUsable usable, ref bool __result)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.IsCustomRole())
        {
            if (usable.TryCast<Vent>() != null)
            {
                __result = true;
                return false;
            }
        }
        
        return true;
    }*/
    
    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(ModRole), nameof(ModRole.FindClosestTarget))]
    public static bool FindClosestPlayerPatch(ModRole __instance, ref PlayerControl __result)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole())
        {
            List<PlayerControl> playersInAbilityRangeSorted = __instance.GetPlayersInAbilityRangeSorted(RoleBehaviour.GetTempPlayerList());
            if (playersInAbilityRangeSorted.Count <= 0)
            {
                __result = null;
                return false;
            }
            __result =  playersInAbilityRangeSorted.ToArray()[0];
            return false;
        }
        
        return true;
    }*/

    /*[HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(bool))]
    public static void HideButtonsPatch(HudManager __instance, [HarmonyArgument(0)] bool isActive) =>
        ToggleButtons(__instance, isActive);*/
    
    /*[HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void HideButtonsPatch2(HudManager __instance, [HarmonyArgument(2)] bool isActive) =>
        HideButtons(__instance, isActive);*/

    [RegisterEventListener(EventType.HudSetActive)]
    public static void ToggleButtons(object sender, HudEventManager.HudSetActiveEventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("HideButtons");
        var customRole = PlayerControl.LocalPlayer.GetCustomRole();
        if (customRole != null)
        {
            args.Hud.AbilityButton.gameObject.SetActive(false);
            args.Hud.ImpostorVentButton.gameObject.SetActive(customRole.CanVent() && args.Active);
            args.Hud.KillButton.gameObject.SetActive(customRole.CanKill() && args.Active);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static bool CheckMurderWithRolePatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        __instance.logger.Debug(string.Format("Checking if {0} murdered {1}", __instance.PlayerId, (target == null) ? "null player" : target.PlayerId.ToString()), null);
        __instance.isKilling = false;
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
        {
            return false;
        }
        if (!target || __instance.Data.IsDead || !(__instance.Data.Role.IsImpostor ||
                                                   (__instance.GetCustomRole() != null && __instance.GetCustomRole().CanKill(target))) || __instance.Data.Disconnected)
        {
            int num = target ? ((int)target.PlayerId) : -1;
            __instance.logger.Warning(string.Format("Bad kill from {0} to {1}", __instance.PlayerId, num), null);
            __instance.RpcMurderPlayer(target, false);
            return false;
        }
        GameData.PlayerInfo data = target.Data;
        if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
        {
            __instance.logger.Warning("Invalid target data for kill", null);
            __instance.RpcMurderPlayer(target, false);
            return false;
        }
        if (MeetingHud.Instance)
        {
            __instance.logger.Warning("Tried to kill while a meeting was starting", null);
            __instance.RpcMurderPlayer(target, false);
            return false;
        }
        __instance.isKilling = true;
        __instance.RpcMurderPlayer(target, true);
        return false;
    }

    [RegisterEventListener(EventType.PlayerDied)]
    public static void ChangePlayerNameOnDeathListener(object sender, PlayerEventManager.PlayerDiedEventArgs args)
    {
        if (args.DeadPlayer.IsLocal())
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                PlayerNameColor.Set(player);
            }
        }
    }
}