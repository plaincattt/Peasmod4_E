using System;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.Roles.Impostor;
using Reactor.Networking.Attributes;
using UnityEngine;
using EventType = Peasmod4.API.Events.EventType;

namespace Peasmod4.Roles.Abilities;

public class StopMovement
{
    public static bool IsMovementStopped;
    public static Predicate<PlayerControl> IsAffected = player => !player.IsCustomRole<Glaciator>();

    [RegisterEventListener(EventType.GameJoined)]
    public static void OnGameJoined(object sender, GameEventManager.GameJoinedEventArgs args)
    {
        IsMovementStopped = false;
    }

    [RegisterEventListener(EventType.HudUpdate)]
    public static void OnUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        var player = PlayerControl.LocalPlayer;
        if (IsMovementStopped && IsAffected.Invoke(player) && !player.Data.IsDead)
        {
            player.moveable = false;
            if (Minigame.Instance != null)
                Minigame.Instance.ForceClose();
            
            player.MyPhysics.ResetMoveState();
            player.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    [MethodRpc((uint) CustomRpcCalls.StopMovement)]
    public static void RpcToggleMovement(PlayerControl sender)
    {
        IsMovementStopped = !IsMovementStopped;
        if (!IsMovementStopped)
            PlayerControl.LocalPlayer.moveable = true;
    }
}