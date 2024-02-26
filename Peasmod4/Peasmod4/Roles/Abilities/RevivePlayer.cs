using Reactor.Networking.Attributes;

namespace Peasmod4.Roles.Abilities;

public static class RevivePlayer
{
    [MethodRpc((uint) CustomRpcCalls.RevivePlayer)]
    public static void RpcRevive(this PlayerControl sender)
    {
        sender.Revive();
    }
}