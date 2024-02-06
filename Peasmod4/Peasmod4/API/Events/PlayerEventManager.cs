using System;
using System.ComponentModel;

namespace Peasmod4.API.Events;

public class PlayerEventManager
{
    public static EventHandler<PlayerDiedEventArgs> PlayerDiedEventHandler;
    public class PlayerDiedEventArgs : EventArgs
    {
        public PlayerControl DeadPlayer;

        public PlayerDiedEventArgs(PlayerControl deadPlayer)
        {
            DeadPlayer = deadPlayer;
        }
    }
    
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
    
    public static EventHandler<CanPlayerBeMurderedEventArgs> CanPlayerBeMurderedEventHandler;
    public class CanPlayerBeMurderedEventArgs : CancelEventArgs
    {
        public PlayerControl Killer;
        public PlayerControl Victim;

        public CanPlayerBeMurderedEventArgs(PlayerControl killer, PlayerControl victim)
        {
            Killer = killer;
            Victim = victim;
        }
    }

    public static EventHandler<PlayerCompletedTaskEventArgs> PlayerCompletedTaskEventHandler;
    public class PlayerCompletedTaskEventArgs : EventArgs
    {
        public PlayerControl Player;
        public PlayerTask Task;

        public PlayerCompletedTaskEventArgs(PlayerControl player, PlayerTask task)
        {
            Player = player;
            Task = task;
        }
    }
}