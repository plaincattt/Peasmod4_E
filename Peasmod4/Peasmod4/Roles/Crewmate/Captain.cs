using System;
using System.Reflection;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using UnityEngine;

namespace Peasmod4.Roles.Crewmate;

#if !API
[RegisterCustomRole]
#endif
public class Captain : CustomRole
{
    public Captain(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnStart;

        CallCooldownOption =
            new CustomNumberOption("CaptainCallingCooldown", "Calling cooldown", 10, NumberSuffixes.Seconds, 1, new FloatRange(10, 60));
        RoleOption = new CustomRoleOption(this, true, CallCooldownOption);
    }

    public override string Name => "Captain";
    public override Sprite Icon => ResourceManager.CallMeetingButton;
    public override string Description => "Keep your crew safe";
    public override string LongDescription => "";
    public override string TaskHint => "Keep your crew safe";
    public override Color Color => Palette.LightBlue;
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Crewmate;
    public override bool HasToDoTasks => true;

    public CustomButton CallButton;
    public CustomNumberOption CallCooldownOption;
    public CustomRoleOption RoleOption;

    public void OnStart(object sender, EventArgs args)
    {
        CallButton = new CustomButton("CaptainCall", () =>
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        }, "Call", Icon, player => player.IsCustomRole(this) && !player.Data.IsDead, _ => true, new CustomButton.CustomButtonOptions(CallCooldownOption.Value));
    }
}