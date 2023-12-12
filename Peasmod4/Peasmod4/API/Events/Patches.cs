using HarmonyLib;

namespace Peasmod4.API.Events;

[HarmonyPatch]
public class Patches
{
    #region GameEvents
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static void GameStartPatch()
    {
        PeasmodPlugin.Logger.LogInfo("test");
        //GameEventManager.GameStartEventHandler?.Invoke(null, EventArgs.Empty);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void GameEndPatch([HarmonyArgument(0)] EndGameResult result)
    {
        PeasmodPlugin.Logger.LogInfo("GameEnd");
        GameEventManager.GameEndEventHandler?.Invoke(null, new GameEventManager.GameEndEventArgs(result));
    }
    #endregion

    #region HudEvents
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        HudEventManager.HudUpdateEventHandler?.Invoke(null, new HudEventManager.HudUpdateEventArgs(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(bool))]
    public static void HudManagerSetActivePatch(HudManager __instance, [HarmonyArgument(0)] bool isActive)
    {
        HudEventManager.HudSetActiveEventHandler?.Invoke(null, new HudEventManager.HudSetActiveEventArgs(__instance, isActive));
    }
    #endregion
    
    #region MeetingEvents
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public static void MeetingEndPatch(ExileController __instance, [HarmonyArgument(0)] GameData.PlayerInfo exiled,
        [HarmonyArgument(1)] bool tie)
    {
        MeetingEventManager.MeetingEndEventHandler?.Invoke(null, new MeetingEventManager.MeetingEndEventArgs(exiled, tie));
    }
    #endregion
    
    #region PlayerEvents
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static void PlayerExiledPatch(PlayerControl __instance)
    {
        PlayerEventManager.PlayerExiledEventHandler?.Invoke(null, new PlayerEventManager.PlayerExiledEventArgs(__instance));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void PlayerMurderedPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl victim, [HarmonyArgument(1)] MurderResultFlags flags)
    {
        PlayerEventManager.PlayerMurderedEventHandler?.Invoke(null, new PlayerEventManager.PlayerMurderedEventArgs(__instance, victim, flags));
    }
    #endregion
}