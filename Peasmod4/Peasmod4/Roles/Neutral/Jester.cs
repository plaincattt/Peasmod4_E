using System;
using System.Collections.Generic;
using System.Reflection;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Networking;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace Peasmod4.Roles.Neutral;

[RegisterCustomRole]
public class Jester : CustomRole
{
    public Jester(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;
        PlayerEventManager.PlayerExiledEventHandler += OnPlayerExiled;

        RoleOption = new CustomRoleOption(this, true);

        CustomEndReason = CustomEndGameManager.RegisterCustomEndReason();
    }

    public override string Name => "Jester";
    public override string Description => "Trick the crew";
    public override string LongDescription => "";
    public override string TaskText => "Trick the crew into voting you out";
    public override Color Color => new Color(136f / 255f, 31f / 255f, 136f / 255f);
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Alone;
    public override bool HasToDoTasks => false;

    public CustomRoleOption RoleOption;

    public static GameOverReason CustomEndReason;

    public void OnGameStart(object sender, EventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("test2Jester");
    }
    
    public void OnPlayerExiled(object sender, PlayerEventManager.PlayerExiledEventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("test3Jester");
        if (args.ExiledPlayer.IsCustomRole(this) && args.ExiledPlayer.IsLocal())
        {
            Rpc<RpcEndGame>.Instance.Send(new RpcEndGame.Data(CustomEndReason, new List<PlayerControl>()
                {
                    PlayerControl.LocalPlayer
                }, "Jester wins", Color));
        }
    }
}