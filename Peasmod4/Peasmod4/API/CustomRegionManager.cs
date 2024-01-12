using System.Collections.Generic;
using HarmonyLib;

namespace Peasmod4.API;

[HarmonyPatch]
public class CustomRegionManager
{
    public static List<StaticHttpRegionInfo> Regions = new List<StaticHttpRegionInfo>();

    public static void AddRegion(string name, string ip, ushort port)
    {
        Regions.Add(new StaticHttpRegionInfo(name, StringNames.NoTranslation, ip, new ServerInfo[] { new ServerInfo(name + "-1", ip, port, false) }));
    }

    internal static void AddCustomRegions()
    {
        foreach (var staticHttpRegionInfo in Regions)
        {
            ServerManager.Instance.AddOrUpdateRegion(staticHttpRegionInfo.Cast<IRegionInfo>()); 
        }
    }
}