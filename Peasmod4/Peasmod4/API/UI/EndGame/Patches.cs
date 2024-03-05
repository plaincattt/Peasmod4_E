using System.Collections.Generic;
using HarmonyLib;
using Peasmod4.API.Roles;
using TMPro;
using UnityEngine;

namespace Peasmod4.API.UI.EndGame;

[HarmonyPatch]
public class Patches
{
    public static Dictionary<string, string> TempRoleRevealText = new ();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void SetRoleRevealTextPatch(AmongUsClient __instance)
    {
        TempRoleRevealText.Clear();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            var role = player.Data.Role;
            TempRoleRevealText.Add(player.name, role.NameColor.ToTextColor() + role.NiceName + "</color>");
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static void ReplaceWinnersPatch(AmongUsClient __instance, [HarmonyArgument(0)] EndGameResult endGameResult)
    {
        /*if (CustomEndGameManager.IsCustom)
        {
            TempData.winners.Clear();
            CustomEndGameManager.WinningPlayers.ForEach(player =>
                TempData.winners.Add(new WinningPlayerData(player.Data)));
        }*/

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
                foreach (var player in PlayerControl.AllPlayerControls) // .WrapToSystem().FindAll(player => player.IsCustomRole(customRole))
                {
                    var _ = false;
                    PeasmodPlugin.Logger.LogInfo(customRole.Name + ": " + player.name + " - " + customRole.DidWin(endGameResult.GameOverReason, player, ref _));
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
        var endReason = CustomEndGameManager.GetCustomEndReason(TempData.EndReason);
        if (endReason != null)
        {
            var reasonText = GameObject.Instantiate(__instance.WinText, __instance.WinText.transform.parent);
            reasonText.name = "CustomReasonText";
            reasonText.transform.localPosition = new Vector3(0f, 2.7f, -14f);
            reasonText.text = "<size=50%>" + endReason.ReasonText + "</size>";
            
            if (endReason.Color.HasValue)
            {
                __instance.BackgroundBar.material.SetColor("_Color", endReason.Color.Value);
                reasonText.color = endReason.Color.Value;
            }
        }

        var roleTextObject = new GameObject("RoleRevealText");
        roleTextObject.layer = LayerMask.NameToLayer("UI");
        
        var aspectPos = roleTextObject.AddComponent<AspectPosition>();
        aspectPos.Alignment = AspectPosition.EdgeAlignments.Left;
        aspectPos.DistanceFromEdge = new Vector3(10.2f, -2.05f, -13f);
        aspectPos.updateAlways = true;

        var scroller = roleTextObject.AddComponent<Scroller>();
        scroller.allowX = false;
        scroller.allowY = true;
        scroller.transform.localScale = Vector3.one;
        scroller.active = true;
        scroller.velocity = new Vector2(0, 0);
        scroller.ContentYBounds = new FloatRange(0, (TempRoleRevealText.Count - 12) * (0.25f));
        scroller.enabled = true;

        var inner = new GameObject("Inner");
        inner.transform.parent = roleTextObject.transform;
        inner.transform.localPosition = new Vector3(0f, 0f);
        scroller.Inner = inner.transform;

        var textChild = new GameObject("RoleText");
        textChild.transform.parent = inner.transform;
        
        var roleText = textChild.AddComponent<TextMeshPro>();
        roleText.fontSize = 2f;
        roleText.lineSpacing = -20f;
        foreach (var keyValuePair in TempRoleRevealText)
        {
            roleText.text += keyValuePair.Key + (TempData.winners.WrapToSystem().Find(data => data.PlayerName == keyValuePair.Key) == null ? "" : " (\u2605)") + ": " + keyValuePair.Value + "\n";
        }
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.DidHumansWin))]
    public static bool DidHumansWinPatch([HarmonyArgument(0)] GameOverReason gameOverReason, ref bool __result)
    {
        var customReason = CustomEndGameManager.GetCustomEndReason(gameOverReason);
        if (customReason != null)
        {
            __result = customReason.CrewWon;
            return false;
        }

        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.DidImpostorsWin))]
    public static bool DidImpostorsWinPatch([HarmonyArgument(0)] GameOverReason gameOverReason, ref bool __result)
    {
        var customReason = CustomEndGameManager.GetCustomEndReason(gameOverReason);
        if (customReason != null)
        {
            __result = customReason.ImpostorWon;
            return false;
        }

        return true;
    }
}