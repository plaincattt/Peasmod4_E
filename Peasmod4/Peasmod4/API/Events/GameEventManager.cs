using System;

namespace Peasmod4.API.Events;

public class GameEventManager
{
    public static EventHandler GameStartEventHandler;
    
    public static EventHandler<GameEndEventArgs> GameEndEventHandler;
    public class GameEndEventArgs : EventArgs
    {
        public EndGameResult Result;
        
        public GameEndEventArgs(EndGameResult result)
        {
            Result = result;
        }
    }
}