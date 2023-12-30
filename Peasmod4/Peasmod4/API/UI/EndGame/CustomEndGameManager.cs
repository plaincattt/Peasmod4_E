using System.Collections.Generic;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Networking;
using Reactor.Networking.Rpc;
using UnityEngine;
using EventType = Peasmod4.API.Events.EventType;

namespace Peasmod4.API.UI.EndGame;

public class CustomEndGameManager
{
    public static int EndReasonsId;
    public static List<CustomEndReason> EndReasons = new List<CustomEndReason>();

    public static CustomEndReason GetCustomEndReason(GameOverReason gameOverReason) =>
        EndReasons.Find(endReason => endReason.EndReason == gameOverReason);
    
    public static bool IsCustomEndReason(GameOverReason gameOverReason) => GetCustomEndReason(gameOverReason) != null;

    public static CustomEndReason RegisterCustomEndReason(string reasonText, Color? color, bool crewWon,
        bool impostorWon)
    {
        var reason =
            new CustomEndReason((GameOverReason)10 + EndReasonsId++, reasonText, color, crewWon, impostorWon);
        EndReasons.Add(reason);
        return reason;
    }

    [RegisterEventListener(EventType.GameEnd)]
    public static void OnGameEnd(object sender, GameEventManager.GameEndEventArgs args)
    {
        EndReasons.Clear();
    }

    public class CustomEndReason
    {
        public GameOverReason EndReason;
        public string ReasonText;
        public Color? Color;
        public bool CrewWon;
        public bool ImpostorWon;

        public CustomEndReason(GameOverReason endReason, string reasonText, Color? color, bool crewWon, bool impostorWon)
        {
            EndReason = endReason;
            ReasonText = reasonText;
            Color = color;
            CrewWon = crewWon;
            ImpostorWon = impostorWon;
        }

        public void Trigger()
        {
            Rpc<RpcEndGame>.Instance.Send(new RpcEndGame.Data(EndReason));
        }
    }
}