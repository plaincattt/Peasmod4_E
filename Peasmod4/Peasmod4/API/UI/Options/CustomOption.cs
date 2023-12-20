using System.Reflection;

namespace Peasmod4.API.UI.Options;

public abstract class CustomOption
{
    public Assembly Assembly { get; internal set; }
        
    public string Id { get; internal set; }
    
    public string Title { get; set; }

    public bool HudVisible { get; set; } = true;
        
    public bool MenuVisible { get; set; } = true;
        
    public bool AdvancedRoleOption { get; set; }
    
    public bool AdvancedVanillaOption { get; set; }

    public string HudFormat { get; set; } = "{0}";
        
    public OptionBehaviour Option { get; internal set; }

    public abstract OptionBehaviour CreateOption();
    
    public CustomOption(string title)
    {
        Title = title;
    }
}