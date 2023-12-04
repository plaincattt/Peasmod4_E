using System.Collections.Generic;

namespace Peasmod4.API.UI.Buttons;

public class CustomButtonManager
{
    public static List<CustomButton> AllButtons = new List<CustomButton>();
    public static List<CustomButton> VisibleButtons = AllButtons.FindAll(button => button.CouldBeUsed());
}