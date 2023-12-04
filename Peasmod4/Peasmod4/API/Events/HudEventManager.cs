using System;

namespace Peasmod4.API.Events;

public class HudEventManager
{
    public static EventHandler<HudUpdateEventArgs> HudUpdateEventHandler;
    public class HudUpdateEventArgs : EventArgs
    {
        public HudManager Hud;
        
        public HudUpdateEventArgs(HudManager hud)
        {
            Hud = hud;
        }
    }

    public static EventHandler<HudSetActiveEventArgs> HudSetActiveEventHandler;
    public class HudSetActiveEventArgs : EventArgs
    {
        public HudManager Hud;
        public bool Active;

        public HudSetActiveEventArgs(HudManager hud, bool active)
        {
            Hud = hud;
            Active = active;
        }
    }
}