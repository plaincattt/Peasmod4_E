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

namespace Peasmod4.Roles.Impostor;

#if !API
[RegisterCustomRole]
#endif
public class Ninja : CustomRole
{
    public Ninja(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;
        
        HideCooldown = new CustomNumberOption("NinjaHideCooldown", "Hide cooldown", 20f, NumberSuffixes.Seconds, 2.5f,
            new FloatRange(0, 100), true);
        HideDuration = new CustomNumberOption("NinjaHideDuration", "Hide duration", 5f, NumberSuffixes.Seconds, 2.5f,
            new FloatRange(0, 50), true);
        RoleOption = new CustomRoleOption(this, true, HideCooldown, HideDuration);
    }

    public override string Name => "Ninja";
    public override Sprite Icon => ResourceManager.TurnInvisibleButton;
    public override string Description => "You can go invisible";
    public override string LongDescription => "";
    public override string TaskHint => "Go invisbile and kill the crewmates secretly";
    public override Color Color => Palette.ImpostorRed;
    public override Enums.Visibility Visibility => Enums.Visibility.Impostor;
    public override Enums.Team Team => Enums.Team.Impostor;
    public override bool HasToDoTasks => false;
    
    public CustomNumberOption HideCooldown;
    public CustomNumberOption HideDuration;
    public CustomRoleOption RoleOption;
    public CustomButton TurnInvisibleButton;

    public void OnGameStart(object sender, EventArgs args)
    {
        TurnInvisibleButton = new CustomButton("NinjaHide", () =>
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