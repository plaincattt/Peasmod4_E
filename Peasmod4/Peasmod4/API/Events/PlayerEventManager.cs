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
    
    public static EventHandler<PlayerMurderedEventArgs> PlayerMurderedEventHandler;
    public class PlayerMurderedEventArgs : EventArgs
    {
        public PlayerControl Killer;
        public PlayerControl Victim;
        public MurderResultFlags Flags;

        public PlayerMurderedEventArgs(PlayerControl killer, PlayerControl victim, MurderResultFlags flags)
        {
            Killer = killer;
            Victim = victim;
            Flags = flags;
        }
    }
}