using System.Collections.Generic;
using Hazel;
using Peasmod4.API.Events;
using Peasmod4.API.UI.EndGame;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace Peasmod4.API.Networking;

[RegisterCustomRpc((uint) CustomRpcCalls.EndGame)]
public class RpcEndGame : PlayerCustomRpc<PeasmodPlugin, RpcEndGame.Data>
{
    public RpcEndGame(PeasmodPlugin plugin, uint id) : base(plugin, id)
    {
    }
    
    public readonly struct Data
    {
        public readonly GameOverReason EndReason;

        public Data(GameOverReason endReason)
        {
            EndReason = endReason;
        }
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write((byte) data.EndReason);
        /*writer.Write(data.WinningPlayers.Count);
        foreach (var winningPlayer in data.WinningPlayers)
        {
            writer.Write(winningPlayer.PlayerId);
        }
        writer.Write(data.Reason);

        var flag = data.Color.HasValue;
        writer.Write(flag);
        if (flag)
        {
            writer.Write(data.Color.Value.r);
            writer.Write(data.Color.Value.g);
            writer.Write(data.Color.Value.b);
            writer.Write(data.Color.Value.a);
        }*/
    }

    public override Data Read(MessageReader reader)
    {
        PeasmodPlugin.Logger.LogInfo("Received RpcEndGame");
        var endReason = (GameOverReason)reader.ReadByte();
        /*var list = new List<PlayerControl>();
        var count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var player = reader.ReadByte().GetPlayer();
            PeasmodPlugin.Logger.LogInfo("Received winner " + player.name);
            list.Add(player);
        }

        var reason = reader.ReadString();

        var hasColor = reader.ReadBoolean();
        Color? color = null;
        if (hasColor)
        {
            color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }*/
        return new Data(endReason);
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            Utility.DoAfterTimeout(() => GameManager.Instance.RpcEndGame(data.EndReason, false), 1f);
        }
    }
}