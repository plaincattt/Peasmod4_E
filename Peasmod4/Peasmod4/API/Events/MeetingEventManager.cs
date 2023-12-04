using System;

namespace Peasmod4.API.Events;

public class MeetingEventManager
{
    public static EventHandler<MeetingEndEventArgs> MeetingEndEventHandler;
    public class MeetingEndEventArgs : EventArgs
    {
        public GameData.PlayerInfo ExiledPlayer;
        public bool Tie;

        public MeetingEndEventArgs(GameData.PlayerInfo exiledPlayer, bool tie)
        {
            ExiledPlayer = exiledPlayer;
            Tie = tie;
        }
    }
}