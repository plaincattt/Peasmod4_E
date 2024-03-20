using System;
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
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
            GameEventManager.GameStartEventHandler?.Invoke(null, EventArgs.Empty);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    public static void GameJoinedPatch([HarmonyArgument(0)] string lobbyCode)
    {
        GameEventManager.GameJoinedEventHandler?.Invoke(null, new GameEventManager.GameJoinedEventArgs(lobbyCode));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.EndGame))]
    public static void GameEndPatch()//([HarmonyArgument(0)] EndGameResult result)
    {
        PeasmodPlugin.Logger.LogInfo("GameEnd");
        GameEventManager.GameEndEventHandler?.Invoke(null, new GameEventManager.GameEndEventArgs(TempData.EndReason));
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
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    public static void PlayerDiedPatch(PlayerControl __instance)
    {
        PlayerEventManager.PlayerDiedEventHandler?.Invoke(null, new PlayerEventManager.PlayerDiedEventArgs(__instance));
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static void PlayerExiledPatch(PlayerControl __instance)
    {
        PlayerEventManager.PlayerExiledEventHandler?.Invoke(null, new PlayerEventManager.PlayerExiledEventArgs(__instance));
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    public static bool CanPlayerBeMurderedPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl victim)
    {
        var result = true;
        
        var _event = PlayerEventManager.CanPlayerBeMurderedEventHandler;
        if (_event != null)
        {
            var args = new PlayerEventManager.CanPlayerBeMurderedEventArgs(__instance, victim);
            foreach (var @delegate in _event.GetInvocationList())
            {
                var t = (EventHandler<PlayerEventManager.CanPlayerBeMurderedEventArgs>)@delegate;
                t(null, args);
                if (args.Cancel)
                {
                    result = false;
                    break;
                }
            }
        }
        //PlayerEventManager.CanPlayerBeMurderedEventHandler?.Invoke(null, new PlayerEventManager.PlayerMurderedEventArgs(__instance, victim, flags));
        return result;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static void PlayerMurderedPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl victim, [HarmonyArgument(1)] MurderResultFlags flags)
    {
        PlayerEventManager.PlayerMurderedEventHandler?.Invoke(null, new PlayerEventManager.PlayerMurderedEventArgs(__instance, victim, flags));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameData), nameof(GameData.CompleteTask))]
    public static void PlayerCompleteTaskPatch(GameData __instance, [HarmonyArgument(0)] PlayerControl player,
        [HarmonyArgument(1)] uint id)
    {
        PlayerEventManager.PlayerCompletedTaskEventHandler?.Invoke(null, new PlayerEventManager.PlayerCompletedTaskEventArgs(player, player.myTasks.WrapToSystem().Find(t => t.Id == id)));
    }
    #endregion
}