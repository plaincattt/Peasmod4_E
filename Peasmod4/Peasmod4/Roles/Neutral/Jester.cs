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

#if !API
[RegisterCustomRole]
#endif
public class Jester : CustomRole
{
    public Jester(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;
        PlayerEventManager.PlayerExiledEventHandler += OnPlayerExiled;

        CanVentOption = new CustomToggleOption("JesterCanVent", "Can vent", false);
        RoleOption = new CustomRoleOption(this, true, CanVentOption);
    }

    public override string Name => "Jester";
    public override string Description => "Trick the crew";
    public override string LongDescription => "";
    public override string TaskHint => "Trick the crew into voting you out";
    public override Color Color => new Color(136f / 255f, 31f / 255f, 136f / 255f);
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Alone;
    public override bool HasToDoTasks => false;
    public override bool CanVent() => CanVentOption != null && CanVentOption.Value;

    public CustomRoleOption RoleOption;
    public CustomToggleOption CanVentOption;

    public static Dictionary<byte, CustomEndGameManager.CustomEndReason> EndReasons =
        new Dictionary<byte, CustomEndGameManager.CustomEndReason>();
    
    public override bool DidWin(GameOverReason gameOverReason, PlayerControl player, ref bool overrides)
    {
        return EndReasons.ContainsKey(player.PlayerId) && EndReasons[player.PlayerId].EndReason == gameOverReason;
    }
    
    public void OnGameStart(object sender, EventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("test2Jester");
        EndReasons.Clear();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.IsCustomRole(this))
            {
                PeasmodPlugin.Logger.LogInfo("Registered Reason: " + player.name);
                EndReasons.Add(player.PlayerId,
                    CustomEndGameManager.RegisterCustomEndReason("Jester won", Color, false, false));
            }
        }
    }
    
    public void OnPlayerExiled(object sender, PlayerEventManager.PlayerExiledEventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("test3Jester");
        if (args.ExiledPlayer.IsCustomRole(this) && args.ExiledPlayer.IsLocal())
        {
            EndReasons[args.ExiledPlayer.PlayerId].Trigger();
        }
    }
}