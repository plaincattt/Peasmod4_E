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
public class Undertaker : CustomRole
{
    public Undertaker(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;

        RoleOption = new CustomRoleOption(this, true);
    }

    public override string Name => "Undertaker";
    public override string Description => "Drag the bodies away";
    public override string LongDescription => "";
    public override Color Color => Palette.ImpostorRed;
    public override Enums.Visibility Visibility => Enums.Visibility.Impostor;
    public override Enums.Team Team => Enums.Team.Impostor;
    public override bool HasToDoTasks => false;

    public CustomButton DragBodyButton;
    public CustomButton DropBodyButton;
    public CustomRoleOption RoleOption;
    
    public void OnGameStart(object sender, EventArgs args)
    {
        DragBodyButton = new CustomButton("Undertaker-DragBody", () =>
        {
            DragBody.RpcDragBody(PlayerControl.LocalPlayer, true, DragBodyButton.ObjectTarget.GetComponent<DeadBody>().ParentId);
        }, "Drag", ResourceManager.DragBodyButton, player => player.IsCustomRole(this) && !player.Data
            .IsDead, player => !player.IsDraggingABody(), new CustomButton.CustomButtonOptions(targetType: CustomButton.CustomButtonOptions.TargetType.Object, objectTargetSelector:
            () => PlayerControl.LocalPlayer.FindNearestObject(obj => obj.GetComponent<DeadBody>(), 1f), targetOutline: Color));
        
        DropBodyButton = new CustomButton("Undertaker-DropBody", () =>
        {
            DragBody.RpcDragBody(PlayerControl.LocalPlayer, false, byte.MaxValue);
        }, "Drop", ResourceManager.DropBodyButton, player => player.IsCustomRole(this) && !player.Data
            .IsDead, player => player.IsDraggingABody());
    }
}