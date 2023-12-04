using System.Collections.Generic;
using UnityEngine;

namespace Peasmod4.API.UI.EndGame;

public class CustomEndGameManager
{
    public static List<PlayerControl> WinningPlayers;
    public static string Reason;
    public static Color? Color;
    public static bool IsCustom;

    public static void Reset()
    {
        WinningPlayers = new List<PlayerControl>();
        Reason = null;
        Color = null;
        IsCustom = false;
    }

    public static int CustomEndReason;
    public static GameOverReason RegisterCustomEndReason() => (GameOverReason)10 + CustomEndReason++;
}