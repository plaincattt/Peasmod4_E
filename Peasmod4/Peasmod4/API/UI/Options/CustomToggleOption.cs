using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Peasmod4.API.Networking;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Object = UnityEngine.Object;

namespace Peasmod4.API.UI.Options;

public class CustomToggleOption : CustomOption
{
    public bool Value { get; private set; }
        
    private ConfigEntry<bool> _configEntry;
    
    public delegate void OnValueChangedHandler(CustomToggleOptionValueChangedArgs args);

    public event OnValueChangedHandler OnValueChanged;

    public class CustomToggleOptionValueChangedArgs
    {
        public CustomToggleOption Option;

        public bool OldValue;

        public bool NewValue;
            
        public CustomToggleOptionValueChangedArgs(CustomToggleOption option, bool oldValue, bool newValue)
        {
            Option = option;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
    
    public void SetValue(bool newValue)
    {
        PeasmodPlugin.Logger.LogInfo(newValue);
        var oldValue = Value;
        Value = newValue;
        
        OnValueChanged?.Invoke(new CustomToggleOptionValueChangedArgs(this, oldValue, newValue));

        if (_configEntry != null && AmongUsClient.Instance.AmHost)
            _configEntry.Value = Value;

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, newValue));
        }
    }

    public override OptionBehaviour CreateOption()
    {
        ToggleOption option = Object.Instantiate(CustomOptionManager.ToggleOptionPrefab, CustomOptionManager.ToggleOptionPrefab.transform.parent);
        option.name = $"{Title}-Option";
        option.Title = CustomStringName.CreateAndRegister(Title);
        option.TitleText.text = Title;
        option.CheckMark.enabled = Value;
        option.OnValueChanged = new Action<OptionBehaviour>(optionBehaviour => SetValue(optionBehaviour.GetBool()));

        Option = option;
        
        return option;
    }
    
    public CustomToggleOption(string id, string title, bool defaultValue) : base(title)
    {
        Assembly = Assembly.GetCallingAssembly();
        Id = $"{Assembly.GetName().Name}.ToggleOption.{id}";
        try
        {
            _configEntry = PeasmodPlugin.ConfigFile.Bind("Options", Id, defaultValue);
        }
        catch (Exception e)
        {
            PeasmodPlugin.Logger.LogError($"Error while loading the option \"{title}\": {e.Message}");
        }
            
        Value = _configEntry?.Value ?? defaultValue;
        HudFormat = "{0}: {1}";
            
        CustomOptionManager.CustomOptions.Add(this);
    }
}