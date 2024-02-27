using System;
using System.Collections.Generic;
using System.Linq;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Reactor.Networking.Attributes;
using UnityEngine;
using EventType = Peasmod4.API.Events.EventType;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace Peasmod4.Roles.Abilities;

public static class DragBody
{
    public static Dictionary<byte, byte> CarriedBodies = new Dictionary<byte, byte>();

    public static bool IsDraggingABody(this PlayerControl player) => CarriedBodies.ContainsKey(player.PlayerId);

    [RegisterEventListener(EventType.GameStart)]
    public static void ClearCarriedBodies(object sender, EventArgs args)
    {
        CarriedBodies.Clear();
    }

    [RegisterEventListener(EventType.HudUpdate)]
    public static void OnHudUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        foreach (var (playerId, bodyId) in CarriedBodies)
        {
            var player = playerId.GetPlayer();
            var body = Object.FindObjectsOfType<DeadBody>()?.ToList().Find(body => body.ParentId == bodyId);
            if (body == null)
            {
                if (player.IsLocal())
                {
                    RpcDragBody(player, false, byte.MaxValue);
                }
                continue;
            }
            
            MoveBodyTowardsPlayer(player, body);
        }
    }

    public static void MoveBodyTowardsPlayer(PlayerControl player, DeadBody body)
    {
        if (!player.inVent)
        {
            var pos = player.transform.position;
            pos.Set(pos.x, pos.y, pos.z + .001f);
            var bodyPos = new Vector3(body.TruePosition.x, body.TruePosition.y, pos.z - .001f);
            body.transform.position = Vector3.Lerp(bodyPos, pos, Time.deltaTime + 0.03f);
        }
        else
            body.transform.position = player.transform.position;
    }

    [MethodRpc((uint) CustomRpcCalls.DragBody)]
    public static void RpcDragBody(PlayerControl sender, bool enable, byte bodyId)
    {
        if (enable)
        {
            CarriedBodies.Add(sender.PlayerId, bodyId);
        }
        else
        {
            if (CarriedBodies.ContainsKey(sender.PlayerId))
                CarriedBodies.Remove(sender.PlayerId);
        }
    }
}