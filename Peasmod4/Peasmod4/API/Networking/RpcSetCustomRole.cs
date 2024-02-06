using AmongUs.GameOptions;
using Hazel;
using Peasmod4.API.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace Peasmod4.API.Networking;

[RegisterCustomRpc((uint) CustomRpcCalls.SetRole)]
public class RpcSetCustomRole : PlayerCustomRpc<PeasmodPlugin, RpcSetCustomRole.Data>
{
    public RpcSetCustomRole(PeasmodPlugin plugin, uint id) : base(plugin, id)
    {
    }

    public readonly struct Data
    {
        public readonly PlayerControl Player;
        public readonly RoleTypes Role;

        public Data(PlayerControl player, RoleTypes role)
        {
            Player = player;
            Role = role;
        }
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Player.PlayerId);
        writer.Write((byte) data.Role);
        
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(reader.ReadByte().GetPlayer(), (RoleTypes) reader.ReadByte());
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        data.Player.SetRoleAfterIntro(data.Role);
    }
}