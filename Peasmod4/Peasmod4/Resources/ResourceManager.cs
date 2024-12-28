using UnityEngine;

namespace Peasmod4.Resources;

public class ResourceManager
{
    public static Sprite PlaceholderButton;
    public static Sprite CallMeetingButton;
    public static Sprite TurnInvisibleButton;
    public static Sprite VoteTwiceButton;
    public static Sprite BuildVentButton;
    public static Sprite RevivePlayerButton;
    public static Sprite DragBodyButton;
    public static Sprite DropBodyButton;

    public static Sprite PeasmodLogo;
    
    public static void LoadAssets()
    {
        PlaceholderButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.Placeholder.png");
        CallMeetingButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.CallMeeting.png", 650f);
        TurnInvisibleButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.TurnInvisible.png", 794f);
        VoteTwiceButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.VoteTwice.png", 100f);
        BuildVentButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.BuildVent.png", 552f);
        RevivePlayerButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.Revive.png", 550f);
        DragBodyButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.DragBody.png", 550f);
        DropBodyButton = Utility.CreateSprite("Peasmod4.Resources.Buttons.DropBody.png", 550f);
        
        PeasmodLogo = Utility.CreateSprite("Peasmod4.Resources.Other.Peasmod.png", 100f);
    }
}