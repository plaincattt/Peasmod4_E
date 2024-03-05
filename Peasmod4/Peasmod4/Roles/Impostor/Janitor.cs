using System;
using System.Reflection;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using Peasmod4.Roles.Abilities;
using UnityEngine;

namespace Peasmod4.Roles.Impostor;

#if !API
[RegisterCustomRole]
#endif
public class Janitor : CustomRole
{
    public Janitor(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;

        //CanKillOption = new CustomToggleOption("Janitor.CanKill", "Can kill", false);
        RoleOption = new CustomRoleOption(this, true, CanKillOption);
    }

    public override string Name => "Janitor";
    public override string Description => "Clean dead bodies of the other Impostors";
    public override string LongDescription => "";
    public override Color Color => Palette.ImpostorRed;
    public override Enums.Visibility Visibility => Enums.Visibility.Impostor;
    public override Enums.Team Team => Enums.Team.Impostor;
    public override bool HasToDoTasks => false;
    public override bool CanKill(PlayerControl victim = null)
    {
        return (CanKillOption.Value || Utility.GetImpostors().FindAll(player => !player.IsCustomRole(this)).Count == 0) && base.CanKill(victim);
    }

    public CustomToggleOption CanKillOption = new CustomToggleOption("Janitor.CanKill", "Can kill", false);
    public CustomRoleOption RoleOption;
    public CustomButton CleanButton;

    public void OnGameStart(object sender, EventArgs args)
    {
        CleanButton = new CustomButton("Janitor.CleanButton",
            () => PlayerControl.LocalPlayer.RpcRemoveBody(CleanButton.ObjectTarget.GetComponent<DeadBody>().ParentId),
            "Clean", ResourceManager.PlaceholderButton,
            player => player.IsCustomRole(this) && !player.Data.IsDead, _ => true,
            new CustomButton.CustomButtonOptions(targetType: CustomButton.CustomButtonOptions.TargetType.Object,
                targetOutline: Palette.ImpostorRed,
                objectTargetSelector: () =>
                    PlayerControl.LocalPlayer.FindNearestObject(obj => obj.GetComponent<DeadBody>(), 1f)));
    }
}