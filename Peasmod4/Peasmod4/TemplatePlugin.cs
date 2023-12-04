using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.EndGame;
using Reactor;
using Reactor.Patches;

namespace Peasmod4;

[HarmonyPatch]
[BepInAutoPlugin(ModId, "Peasmod", "4.0.0")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class PeasmodPlugin : BasePlugin
{
    public const string ModId = "xyz.peasplayer.peasmod4";
    
    public static ManualLogSource Logger { get; private set; }

    public static ConfigFile ConfigFile { get; private set; }
    
    public Harmony Harmony { get; } = new(Id);

    public PeasmodPlugin()
    {
        Logger = Log;
        ConfigFile = Config;
        
        RegisterCustomRoleAttribute.Load();
    }
    
    public override void Load()
    {
        ReactorVersionShower.TextUpdated += text =>
        {
            text.text = "Not again\nPeasmod V4";
        };

        GameEventManager.GameEndEventHandler += (_, _) => CustomEndGameManager.EndReasons.Clear();
        
        Harmony.PatchAll();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static void PingPatch(PingTracker __instance)
    {
        __instance.text.text += "\nPeasmod is (maybe) back!";
    }
}