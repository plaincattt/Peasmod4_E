using System;
using System.Linq;
using Il2CppSystem.Text;

namespace Peasmod4.API.UI.Options;

public static class Extensions
{
    public static bool IsCustomOption(this OptionBehaviour option) => GetCustomOption(option) != null;
    
    public static CustomOption GetCustomOption(this OptionBehaviour option)
    {
        var customOption = CustomOptionManager.CustomOptions.Find(custom => custom.Option == option);
        if (customOption != null)
            return customOption;
        
        customOption = CustomOptionManager.CustomRoleOptions.Find(custom => custom.Option == option);
        
        return customOption;
    }
    
    public static void RenderOption(this CustomOption option, StringBuilder builder, string prefix = "")
    {
        switch (option)
        {
            case CustomToggleOption _option:
                builder.AppendLine(prefix + String.Format(_option.HudFormat, _option.Title, _option.Value ? "On" : "Off") + Utility.StringColor.Reset);
                break;
            case CustomNumberOption _option:
                var value = Math.Round(_option.Value, 2);
                builder.AppendLine(prefix + String.Format(_option.HudFormat, _option.Title, _option.ZeroIsInfinity && value == 0f ? "\u221e" : value,
                    _option.Suffix == NumberSuffixes.None ? "" :
                    _option.Suffix == NumberSuffixes.Multiplier ? "x" :
                    _option.Suffix == NumberSuffixes.Seconds ? "s" : "") + Utility.StringColor.Reset);
                break;
            case CustomStringOption _option:
                builder.AppendLine(prefix + String.Format(_option.HudFormat, _option.Title, _option.StringValue) + Utility.StringColor.Reset);
                break;
            /*case CustomOptionHeader _option:
                builder.AppendLine(prefix + String.Format(_option.HudFormat, _option.Title) + Utility.StringColor.Reset);
                break;*/
        }
    }
}