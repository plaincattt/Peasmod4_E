using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
using UnityEngine;

namespace Peasmod4.Roles.Neutral;

#if !API
[RegisterCustomRole]
#endif
public class Jinx : CustomRole
{
    public Jinx(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += OnGameStart;
        HudEventManager.HudUpdateEventHandler += OnUpdate;

        RoleOption = new CustomRoleOption(this, true);
    }

    public override string Name => "Jinx";
    public override string Description => "Survive the longest";
    public override string LongDescription => "";
    public override string TaskHint => "Be one of the last three survivors";
    public override Color Color => new Color(58 / 256f, 13 / 255f, 58 / 255f);
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Alone;
    public override bool HasToDoTasks => false;

    public CustomRoleOption RoleOption;
    
    public Dictionary<byte, CustomEndGameManager.CustomEndReason> EndReasons =
        new Dictionary<byte, CustomEndGameManager.CustomEndReason>();
    public bool MadeWinCall;
    
    public override bool DidWin(GameOverReason gameOverReason, PlayerControl player, ref bool overrides)
    {
        return EndReasons.ContainsKey(player.PlayerId) && EndReasons[player.PlayerId].EndReason == gameOverReason;
    }

    public void OnGameStart(object sender, EventArgs args)
    {
        MadeWinCall = false;
        EndReasons.Clear();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.IsCustomRole(this))
            {
                PeasmodPlugin.Logger.LogInfo("Registered Reason: " + player.name);
                EndReasons.Add(player.PlayerId,
                    CustomEndGameManager.RegisterCustomEndReason("Jinx won", Color, false, false));
            }
        }
    }
    
    public void OnUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        if (PlayerControl.AllPlayerControls.WrapToSystem().Count(p => !p.Data.IsDead && !p.Data.Disconnected) == 3 &&
            PlayerControl.AllPlayerControls.WrapToSystem().Count(p => !p.Data.IsDead && !p.Data.Disconnected && p.Data.Role != null && p.Data.Role.IsImpostor) >=
            1 &&
            PlayerControl.LocalPlayer.IsCustomRole(this) && !PlayerControl.LocalPlayer.Data.IsDead && !MadeWinCall)
        {
            MadeWinCall = true;
            EndReasons[PlayerControl.LocalPlayer.PlayerId].Trigger();
        }
    }
}