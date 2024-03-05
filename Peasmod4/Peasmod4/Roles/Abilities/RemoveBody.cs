using System.Linq;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Peasmod4.Roles.Abilities;

public static class RemoveBody
{
    [MethodRpc((uint) CustomRpcCalls.RemoveBody)]
    public static void RpcRemoveBody(this PlayerControl player, byte bodyId)
    {
        var body = GameObject.FindObjectsOfType<DeadBody>()?.ToList().Find(body => body.ParentId == bodyId);
        if (body == null)
            return;
        
        body.gameObject.Destroy();
    }
}