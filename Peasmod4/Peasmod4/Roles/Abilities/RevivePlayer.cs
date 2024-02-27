using System.Linq;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Peasmod4.Roles.Abilities;

public static class RevivePlayer
{
    [MethodRpc((uint) CustomRpcCalls.RevivePlayer)]
    public static void RpcRevive(this PlayerControl sender)
    {
        sender.Revive();
        Object.FindObjectsOfType<DeadBody>().ToList().Find(body => body.ParentId == sender.PlayerId).gameObject.Destroy();
    }
}