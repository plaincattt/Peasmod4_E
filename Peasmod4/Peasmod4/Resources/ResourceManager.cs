using UnityEngine;

namespace Peasmod4.Resources;

public class ResourceManager
{
    public static Sprite CallMeetingButton;
    public static Sprite TurnInvisibleButton;
    public static Sprite VoteTwiceButton;
    public static Sprite BuildVentButton;
    
    public static void LoadAssets()
    {
        CallMeetingButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.CallMeeting.png", 650f);
        TurnInvisibleButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.TurnInvisible.png", 794f);
        VoteTwiceButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.VoteTwice.png", 100f);
        BuildVentButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.BuildVent.png", 552f);
    }
}