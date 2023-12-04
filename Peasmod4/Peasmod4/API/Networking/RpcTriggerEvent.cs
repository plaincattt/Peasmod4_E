using System;
using Hazel;
using Peasmod4.API.Events;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace Peasmod4.API.Networking;

[RegisterCustomRpc((uint) CustomRpcCalls.TriggerEvent)]
public class RpcTriggerEvent : PlayerCustomRpc<PeasmodPlugin, RpcTriggerEvent.Data>
{
    public RpcTriggerEvent(PeasmodPlugin plugin, uint id) : base(plugin, id)
    {
    }
    
    public readonly struct Data
    {
        public readonly string Event;

        public Data(string _event)
        {
            Event = _event;
        }
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;
    
    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Event);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(reader.ReadString());
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        switch (data.Event)
        {
            case "Start":
                GameEventManager.GameStartEventHandler.Invoke(null, EventArgs.Empty);
                break;
        }
    }
}