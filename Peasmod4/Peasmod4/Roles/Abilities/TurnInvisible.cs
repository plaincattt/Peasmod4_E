using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace Peasmod4.Roles.Abilities;

[HarmonyPatch]
public class TurnInvisible
{
    public static List<byte> InvisiblePlayers = new List<byte>();

    public static void ApplyInvisibility(PlayerControl player, bool enabled)
    {
        if (player.IsLocal())
        {
            var bodyRenderer = player.cosmetics.currentBodySprite.BodySprite;
            bodyRenderer.color =
                bodyRenderer.color.SetAlpha(enabled ? 0.5f : 1f);
            player.SetHatAndVisorAlpha(enabled ? 0.5f : 1f);
            player.cosmetics.skin.layer.color =
                player.cosmetics.skin.layer.color.SetAlpha(enabled ? 0.5f : 1f);
        }
        else
        {
            player.Visible = player.Collider.enabled = !enabled;
        }
    }

    [RegisterEventListener(EventType.GameStart)]
    public static void ResetPlayerList(object sender, EventArgs args)
    {
        InvisiblePlayers.Clear();
    }

    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHatAndVisorAlpha))]
        public static void StopChangingAlphaPatch(PlayerControl __instance, [HarmonyArgument(0)] ref float alpha)
        {
            if (InvisiblePlayers.Contains(__instance.PlayerId) && __instance.IsLocal())
            {
                alpha = 0.5f;
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ResetMoveState))]
        public static void TurnInvisibleAfterResetPatch(PlayerPhysics __instance)
        {
            if (InvisiblePlayers.Contains(__instance.myPlayer.PlayerId))
            {
                ApplyInvisibility(__instance.myPlayer, true);
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerPhysics._CoExitVent_d__55), nameof(PlayerPhysics._CoExitVent_d__55.MoveNext))]
        public static void TurnInvisibleAfterVentPatch(PlayerPhysics._CoExitVent_d__55 __instance)
        {
            TurnInvisibleAfterResetPatch(__instance.__4__this);
        }
    }
    
    [RegisterCustomRpc((uint) CustomRpcCalls.TurnInvisible)]
    public class RpcTurnInvisible : PlayerCustomRpc<PeasmodPlugin, RpcTurnInvisible.Data>
    {
        public RpcTurnInvisible(PeasmodPlugin plugin, uint id) : base(plugin, id)
        {
        }
    
        public readonly struct Data
        {
            public readonly PlayerControl Player;
            public readonly bool Enabled;

            public Data(PlayerControl player, bool enabled)
            {
                Player = player;
                Enabled = enabled;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Player.PlayerId);
            writer.Write(data.Enabled);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadByte().GetPlayer(), reader.ReadBoolean());
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (data.Enabled)
            {
                InvisiblePlayers.Add(data.Player.PlayerId);
            }
            else
            {
                InvisiblePlayers.Remove(data.Player.PlayerId);
            }
            
            ApplyInvisibility(data.Player, data.Enabled);
        }
    }
}