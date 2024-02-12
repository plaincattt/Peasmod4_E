using System;
using System.Reflection;
using AmongUs.GameOptions;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Options;
using UnityEngine;

namespace Peasmod4.Roles.Neutral;

#if !API
[RegisterCustomRole]
#endif
public class Traitor : CustomRole
{
    public Traitor(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;
        PlayerEventManager.PlayerCompletedTaskEventHandler += OnTaskComplete;

        RoleOption = new CustomRoleOption(this, true);
    }

    public override string Name => "Traitor";
    public override string Description => "Betray the crewmates";
    public override string LongDescription => "";
    public override string TaskHint => "Betray the crewmates after you completed your tasks";
    public override Color Color => Palette.ImpostorRed;
    public override Enums.Visibility Visibility => HasBetrayed ? Enums.Visibility.Impostor : Enums.Visibility.NoOne;
    public override Enums.Team Team => HasBetrayed ? Enums.Team.Impostor : Enums.Team.Alone;
    public override bool HasToDoTasks => true;

    public CustomRoleOption RoleOption;
    
    public bool HasBetrayed;
    
    public void OnGameStart(object sender, EventArgs args)
    {
        HasBetrayed = false;
    }

    public void OnTaskComplete(object sender, PlayerEventManager.PlayerCompletedTaskEventArgs args)
    {
        if (args.Player.IsLocal() && args.Player.IsCustomRole(this) && args.Player.AllTasksCompleted())
        {
            args.Player.RpcSetVanillaRole(RoleTypes.Impostor);    
            HasBetrayed = true;
        }
    }
}