using System.Reflection;
using BepInEx.Configuration;
using Peasmod4.API.Networking;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace Peasmod4.API.UI.Options;

public class CustomNumberOption : CustomOption
{
    public float Value;

    public NumberSuffixes Suffix;

    public float Increment;

    public FloatRange Range;

    public bool IsDecimal;

    public bool ZeroIsInfinity;
    
    private ConfigEntry<float> _configEntry;

    public delegate void OnValueChangedHandler(CustomNumberOptionValueChangedArgs args);

    public event OnValueChangedHandler OnValueChanged;

    public class CustomNumberOptionValueChangedArgs
    {
        public CustomNumberOption Option;

        public float OldValue;

        public float NewValue;
            
        public CustomNumberOptionValueChangedArgs(CustomNumberOption option, float oldValue, float newValue)
        {
            Option = option;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
    
    public void SetValue(float newValue)
    {
        var oldValue = Value;
        Value = newValue;
        
        OnValueChanged?.Invoke(new CustomNumberOptionValueChangedArgs(this, oldValue, newValue));

        if (_configEntry != null && AmongUsClient.Instance.AmHost)
            _configEntry.Value = Value;

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, newValue));
        }
        PeasmodPlugin.Logger.LogInfo("1b - " + Title + ": " + newValue + " - " + Range.max + " - " + Range.min + " - " + Range.Contains(newValue));
    }

    public override OptionBehaviour CreateOption()
    {
        PeasmodPlugin.Logger.LogInfo("2a - " + Title + ": " + " - " + Range.max + " - " + Range.min);
        NumberOption option = Object.Instantiate(CustomOptionManager.NumberOptionPrefab, CustomOptionManager.NumberOptionPrefab.transform.parent);
        option.name = $"{Title}-Option";
        option.Title = CustomStringName.CreateAndRegister(Title);
        option.TitleText.text = Title;
        option.Value = Value;
        option.SuffixType = Suffix;
        option.Increment = Increment;
        option.ValidRange = new FloatRange(Range.min, Range.max);
        option.FormatString = IsDecimal ? "0.0#" : "0";
        option.ZeroIsInfinity = ZeroIsInfinity;
        option.OnValueChanged = new System.Action<OptionBehaviour>(optionBehaviour => SetValue(optionBehaviour.GetFloat()));

        Option = option;
        
        PeasmodPlugin.Logger.LogInfo("2b - " + Title + ": " + " - " + Range.max + " - " + Range.min);
        return option;
    }

    public CustomNumberOption(string id, string title, float defaultValue, NumberSuffixes suffix, float increment, FloatRange range, bool isDecimal = false, bool zeroIsInfinity = false) : base(title)
    {
        Assembly = Assembly.GetCallingAssembly();
        Id = $"{Assembly.GetName().Name}.NumberOption.{id}";
        try
        {
            _configEntry = PeasmodPlugin.ConfigFile.Bind("Options", Id, defaultValue);
        }
        catch (Exception e)
        {
            PeasmodPlugin.Logger.LogError($"Error while loading the option \"{title}\": {e.Message}");
        }
            
        Value = _configEntry?.Value ?? defaultValue;
        Suffix = suffix;
        Increment = increment;
        Range = range;
        PeasmodPlugin.Logger.LogInfo(Title + ": " + Range.max  + " - " + Range.min);
        IsDecimal = isDecimal;
        ZeroIsInfinity = zeroIsInfinity;
        HudFormat = "{0}: {1}{2}";
            
        CustomOptionManager.CustomOptions.Add(this);
    }
}