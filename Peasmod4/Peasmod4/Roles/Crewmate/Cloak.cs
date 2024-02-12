using System;
using System.Reflection;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using Peasmod4.Roles.Abilities;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace Peasmod4.Roles.Crewmate;

#if !API
[RegisterCustomRole]
#endif
public class Cloak : CustomRole
{
    public Cloak(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;

        HideCooldown = new CustomNumberOption("CloakHideCooldown", "Hide cooldown", 20f, NumberSuffixes.Seconds, 2.5f,
            new FloatRange(0, 100), true);
        HideDuration = new CustomNumberOption("CloakHideDuration", "Hide duration", 5f, NumberSuffixes.Seconds, 2.5f,
            new FloatRange(0, 50), true);
        RoleOption = new CustomRoleOption(this, true, HideCooldown, HideDuration);
    }

    public override string Name => "Cloak";
    public override Sprite Icon => ResourceManager.TurnInvisibleButton;
    public override string Description => "You can go invisible";
    public override string LongDescription => "";
    public override string TaskHint => "Go invisible and try to catch the impostor";
    public override Color Color => Color.gray;
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Crewmate;
    public override bool HasToDoTasks => true;

    public CustomNumberOption HideCooldown;
    public CustomNumberOption HideDuration;
    public CustomRoleOption RoleOption;
    
    public CustomButton TurnInvisibleButton;

    public void OnGameStart(object sender, EventArgs args)
    {
        TurnInvisibleButton = new CustomButton("CloakHide", () =>
            {
                Rpc<TurnInvisible.RpcTurnInvisible>.Instance.Send(new TurnInvisible.RpcTurnInvisible.Data(PlayerControl.LocalPlayer, true));
            }, "Hide", Utility.CreateSprite("Peasmod4.Resources.Buttons.TurnInvisible.png", 794f), 
            player => player.IsCustomRole(this) && !player.Data.IsDead, _ => true, new CustomButton.CustomButtonOptions(HideCooldown.Value, true, HideDuration.Value,
                () =>
                {
                    Rpc<TurnInvisible.RpcTurnInvisible>.Instance.Send(new TurnInvisible.RpcTurnInvisible.Data(PlayerControl.LocalPlayer, false));
                }));
    }
}