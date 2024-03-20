namespace Peasmod4.API.Events;

public enum EventType
{
    // Game events
    GameStart,
    GameJoined,
    GameEnd,
    // Hud events
    HudUpdate,
    HudSetActive,
    // Meeting events
    MeetingEnd,
    // Player events
    PlayerDied,
    PlayerExiled,
    PlayerMurdered
}