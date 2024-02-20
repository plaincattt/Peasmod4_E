using System.Linq;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace Peasmod4.Roles.Abilities;

public class BuildVent
{
    public static int LastPlacedVent = int.MinValue;
    private static int GetVentId() => ShipStatus.Instance.AllVents.Count;
        
    public static void CreateVent(Vector3 position)
    {
        var realPos = new Vector3(position.x, position.y, position.z + .001f);
        var ventPref = Object.FindObjectOfType<Vent>();
        var vent = Object.Instantiate(ventPref, ventPref.transform.parent);
        vent.Id = GetVentId();
        vent.transform.position = realPos;
        var leftVent = int.MinValue;
        if (LastPlacedVent != int.MinValue)
        {
            leftVent = LastPlacedVent;
        }
            
        vent.Left = leftVent == int.MinValue ? null : ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == leftVent);
        vent.Center = null;
        vent.Right = null;
            
        var allVents = ShipStatus.Instance.AllVents.ToList();
        allVents.Add(vent);
        ShipStatus.Instance.AllVents = allVents.ToArray();
        if (vent.Left != null)
            vent.Left.Right = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == vent.Id);
        LastPlacedVent = vent.Id;
    }

    [MethodRpc((uint) CustomRpcCalls.BuildVent)]
    public static void RpcCreateVent(PlayerControl sender, float x, float y, float z)
    {
        CreateVent(new Vector3(x, y, z));
    }
}