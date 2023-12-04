using System;

namespace Peasmod4.API.Events;

public class PlayerEventManager
{
    public static EventHandler<PlayerExiledEventArgs> PlayerExiledEventHandler;
    public class PlayerExiledEventArgs : EventArgs
    {
        public PlayerControl ExiledPlayer;

        public PlayerExiledEventArgs(PlayerControl exiledPlayer)
        {
            ExiledPlayer = exiledPlayer;
        }
    }
}