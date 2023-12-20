using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
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

    public static CustomToggleOption ShowRolesToDead;
    
    public Harmony Harmony { get; } = new(Id);

    public PeasmodPlugin()
    {
        Logger = Log;
        ConfigFile = Config;
        
        RegisterCustomRoleAttribute.Load();
        RegisterEventListenerAttribute.Load();
    }

    [RegisterEventListener(EventType.GameStart)]
    public static void TestListener(object sender, EventArgs test)
    {
        PeasmodPlugin.Logger.LogInfo("HALLO WELLLLLT");
    }
    
    public override void Load()
    {
        #if !API
        ReactorVersionShower.TextUpdated += text =>
        {
            var versionText = "Not again\nPeasmod V4";
            #if DEV
            versionText += $"\n{Utility.StringColor.Red}Unstable Version!{Utility.StringColor.Reset}";
            #endif
            
            text.text = versionText;
        };
        #endif

        GameEventManager.GameEndEventHandler += (_, _) => CustomEndGameManager.EndReasons.Clear();

        ShowRolesToDead = new CustomToggleOption("ShowRolesToDead", "Show roles to dead", true) { AdvancedVanillaOption = true };
        
        Harmony.PatchAll();
    }

    #if !API
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static void PingPatch(PingTracker __instance)
    {
        var pingText = "\nPeasmod is (maybe) back!";
        #if DEV
        pingText += $"\n{Utility.StringColor.Red}Unstable Version!{Utility.StringColor.Reset}";
        #endif
        __instance.text.text += pingText;
    }
    #endif
}