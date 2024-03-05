using System;
using System.Reflection;
using HarmonyLib;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using UnityEngine;

namespace Peasmod4.Roles.Neutral;

[HarmonyPatch]
#if !API
[RegisterCustomRole]
#endif
public class Zombie : CustomRole
{
    public Zombie(Assembly assembly) : base(assembly)
    {
        Reach = new CustomStringOption("ZombieReach", "Reach", "Small", "Medium", "Long");
        InfectCooldown = new CustomNumberOption("ZombieInfectCooldown", "Infect cooldown", 20f, NumberSuffixes.Seconds,
            2.5f, new FloatRange(10f, 100f), true);
        SpeedMod = new CustomNumberOption("ZombieSpeedMod", "Speed modification", 1f, NumberSuffixes.Multiplier,
            0.25f, new FloatRange(0.25f, 1.5f), true);
        LightMod = new CustomNumberOption("ZombieLightMod", "Vision modification", 1f, NumberSuffixes.Multiplier,
            0.25f, new FloatRange(0.25f, 1.5f), true);
        VisibleToEveryone = new CustomToggleOption("ZombieVisibleToEveryone", "Visible to everyone", false);
        RoleOption = new CustomRoleOption(this, true, Reach, InfectCooldown, SpeedMod, LightMod, VisibleToEveryone);

        GameEventManager.GameStartEventHandler += (_, _) =>
            EndReason = CustomEndGameManager.RegisterCustomEndReason("Zombies infected the crew", ZombieColor, false,
                false);
        HudEventManager.HudUpdateEventHandler += OnUpdate;
    }

    public override string Name => "Zombie";
    public override string Description => "Infect every crewmate";

    public override string LongDescription =>
        "Spread the zombie disease to every other crewmate and turn them into zombies too but watch out! You can't infect impostors because they already have a different disease";

    public override string TaskHint => "Infect every crewmate";
    public override Color Color => ZombieColor;
    public readonly Color ZombieColor = new Color(109 / 255f, 142 / 255f, 74 / 255f);
    public override Enums.Visibility Visibility => VisibleToEveryone.Value ? Enums.Visibility.Everyone : Enums.Visibility.Role;
    public override Enums.Team Team => Enums.Team.Role;
    public override bool HasToDoTasks => false;

    public CustomButton InfectButton;
    public CustomStringOption Reach;
    public CustomNumberOption InfectCooldown;
    public CustomNumberOption SpeedMod;
    public CustomNumberOption LightMod;
    public CustomToggleOption VisibleToEveryone;
    public CustomRoleOption RoleOption;
    public CustomEndGameManager.CustomEndReason EndReason;
    public bool MadeWinCall;

    public override void OnRoleAssigned(PlayerControl player)
    {
        if (!player.IsLocal())
            return;
        
        MadeWinCall = false;
        
        InfectButton = new CustomButton("InfectButton", () =>
            {
                if (InfectButton.PlayerTarget.Data.Role.IsImpostor)
                    return;
                InfectButton.PlayerTarget.RpcSetCustomRole(this);
                
                if (PlayerControl.AllPlayerControls.WrapToSystem().FindAll(player => player != null &&
                        !player.IsCustomRole(this) && !player.Data.IsDead && !player.Data.Role.IsImpostor).Count == 0)
                {
                    EndReason.Trigger();
                    MadeWinCall = true;
                }
            }, "Infect",
            ResourceManager.PlaceholderButton, player => player.IsCustomRole(this),
            player => player.IsCustomRole(this), new CustomButton.CustomButtonOptions(InfectCooldown.Value, targetType: CustomButton.CustomButtonOptions.TargetType.Player, 
                playerTargetSelector: () => PlayerControl.LocalPlayer.FindNearestPlayer(player => !player.IsCustomRole(this) && !player.Data.IsDead, Reach.Value + 1), targetOutline: ZombieColor));
        
        if (PlayerControl.LocalPlayer.IsCustomRole(this))
        {
            PlayerControl.LocalPlayer.MyPhysics.Speed *= SpeedMod.Value;
        }
    }

    public void OnUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        if (AmongUsClient.Instance.AmHost && !MadeWinCall)
        {
            if (PlayerControl.AllPlayerControls.WrapToSystem().FindAll(player => player != null && player.Data != null &&
                    !player.IsCustomRole(this) && !player.Data.IsDead && !player.Data.Role.IsImpostor).Count == 0 && PlayerControl.AllPlayerControls.Count > 1)
            {
                EndReason.Trigger();
                MadeWinCall = true;
            }
        }
    }
    
    public override bool DidWin(GameOverReason gameOverReason, PlayerControl player, ref bool overrides)
    {
        if (EndReason.EndReason == gameOverReason)
        {
            overrides = true;
            return player.IsCustomRole(this);
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    public static void AdjustLightRadius(ShipStatus __instance, ref float __result)
    {
        if (PlayerControl.LocalPlayer.IsCustomRole<Zombie>())
        {
            __result *= CustomRoleManager.GetRole<Zombie>().LightMod.Value;
        }
    }
}