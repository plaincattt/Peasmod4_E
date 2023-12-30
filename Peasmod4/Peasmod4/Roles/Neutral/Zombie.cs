using System;
using System.Reflection;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
using UnityEngine;

namespace Peasmod4.Roles.Neutral;

[RegisterCustomRole]
public class Zombie : CustomRole
{
    public Zombie(Assembly assembly) : base(assembly)
    {
        InfectCooldown = new CustomNumberOption("ZombieInfectCooldown", "Infect cooldown", 20f, NumberSuffixes.Seconds,
            2.5f, new FloatRange(10f, 100f), true);
        RoleOption = new CustomRoleOption(this, true, InfectCooldown);

        GameEventManager.GameStartEventHandler += (_, _) =>
            EndReason = CustomEndGameManager.RegisterCustomEndReason("Zombies infected the crew", ZombieColor, false,
                false);
        HudEventManager.HudUpdateEventHandler += OnUpdate;
    }

    public override string Name => "Zombie";
    public override string Description => "Infect every crewmate";

    public override string LongDescription =>
        "Spread the zombie disease to every other crewmate and turn them into zombies too but watch out! You can't infect impostors because they already have a different disease";

    public override string TaskText => "Infect every crewmate";
    public override Color Color => ZombieColor;
    public readonly Color ZombieColor = new Color(109 / 255f, 142 / 255f, 74 / 255f);
    public override Enums.Visibility Visibility => Enums.Visibility.Role;
    public override Enums.Team Team => Enums.Team.Role;
    public override bool HasToDoTasks => false;
    public override int MaxCount => 1;

    public CustomButton InfectButton;
    public CustomNumberOption InfectCooldown;
    public CustomRoleOption RoleOption;
    public CustomEndGameManager.CustomEndReason EndReason;
    public bool MadeWinCall;

    public override void OnRoleAssigned()
    {
        MadeWinCall = false;
        
        InfectButton = new CustomButton("InfectButton", () =>
            {
                if (InfectButton.PlayerTarget.Data.Role.IsImpostor)
                    return;
                InfectButton.PlayerTarget.RpcSetCustomRole(this);
            }, "Infect",
            Utility.CreateSprite("Peasmod4.Placeholder.png", 128f), player => player.IsCustomRole(this),
            player => player.IsCustomRole(this), new CustomButton.CustomButtonOptions(InfectCooldown.Value, targetType: CustomButton.CustomButtonOptions.TargetType.Player, 
                playerTargetSelector: () => PlayerControl.LocalPlayer.FindNearestPlayer(player => !player.IsCustomRole(this) && !player.Data.IsDead), playerTargetOutline: ZombieColor));
    }

    public void OnUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        if (AmongUsClient.Instance.AmHost && !MadeWinCall)
        {
            if (PlayerControl.AllPlayerControls.WrapToSystem().FindAll(player => player != null &&
                    !player.IsCustomRole(this) && !player.Data.IsDead && !player.Data.Role.IsImpostor).Count == 0)
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
}    