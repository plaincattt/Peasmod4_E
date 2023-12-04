using HarmonyLib;
using Peasmod4.API.Roles;
using UnityEngine;

namespace Peasmod4.API.UI.EndGame;

[HarmonyPatch]
public class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void ReplaceWinnersPatch(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
    {
        if (CustomEndGameManager.IsCustom)
        {
            TempData.winners.Clear();
            CustomEndGameManager.WinningPlayers.ForEach(player =>
                TempData.winners.Add(new WinningPlayerData(player.Data)));
        }

        var overridingRole = CustomRoleManager.Roles.Find(role =>
        {
            var result = false;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var tmp = false;
                role.DidWin(endGameResult.GameOverReason, player, ref tmp);
                if (tmp)
                    result = true;
            }

            return result;
        });
        if (overridingRole != null)
        {
            TempData.winners.Clear();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var _ = false;
                if (overridingRole.DidWin(endGameResult.GameOverReason, player, ref _))
                    TempData.winners.Add(new WinningPlayerData(player.Data));
            }
        }
        else
        {
            foreach (var customRole in CustomRoleManager.Roles)
            {
                foreach (var player in PlayerControl.AllPlayerControls.WrapToSystem().FindAll(player => player.IsCustomRole(customRole)))
                {
                    var _ = false;
                    if (customRole.DidWin(endGameResult.GameOverReason, player, ref _) && TempData.winners.WrapToSystem().Find(data => data.PlayerName == player.name) == null)
                        TempData.winners.Add(new WinningPlayerData(player.Data));
                }
            } 
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static void ChangeColorPatch(EndGameManager __instance)
    {
        if (CustomEndGameManager.IsCustom)
        {
            var reasonText = GameObject.Instantiate(__instance.WinText, __instance.WinText.transform.parent);
            reasonText.name = "CustomReasonText";
            reasonText.transform.localPosition = new Vector3(0f, 2.7f, -14f);
            reasonText.text = "<size=50%>" + CustomEndGameManager.Reason + "</size>";
            
            if (CustomEndGameManager.Color.HasValue)
            {
                __instance.BackgroundBar.material.SetColor("_Color", CustomEndGameManager.Color.Value);
                reasonText.color = CustomEndGameManager.Color.Value;
            }
        }

        CustomEndGameManager.Reset();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EndGameManager._CoBegin_d__18), nameof(EndGameManager._CoBegin_d__18.MoveNext))]
    public static void AnimateReasonText(EndGameManager._CoBegin_d__18 __instance)
    {
        var reasonText = GameObject.Find("CustomReasonText");
        if (reasonText != null)
            reasonText.transform.localPosition =
                __instance.__4__this.WinText.transform.localPosition - new Vector3(0f, 0.8f);
    }
}