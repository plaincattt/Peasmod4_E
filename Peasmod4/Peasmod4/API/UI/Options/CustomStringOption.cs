using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using Peasmod4.API.Networking;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Object = UnityEngine.Object;

namespace Peasmod4.API.UI.Options;

public class CustomStringOption : CustomOption
{
    public int Value;

    public List<StringNames> StringValues;

    public string StringValue => StringValues[Value].GetTranslation();

    private ConfigEntry<int> _configEntry;
    
    public delegate void OnValueChangedHandler(CustomStringOptionValueChangedArgs args);

    public event OnValueChangedHandler OnValueChanged;

    public class CustomStringOptionValueChangedArgs
    {
        public CustomStringOption Option;

        public int OldValue;

        public int NewValue;
            
        public CustomStringOptionValueChangedArgs(CustomStringOption option, int oldValue, int newValue)
        {
            Option = option;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public void SetValue(int newValue)
    {
        var oldValue = Value;
        Value = newValue;
        
        OnValueChanged?.Invoke(new CustomStringOptionValueChangedArgs(this, oldValue, newValue));

        if (_configEntry != null && AmongUsClient.Instance.AmHost)
            _configEntry.Value = Value;

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<RpcUpdateSetting>.Instance.Send(new RpcUpdateSetting.Data(this, newValue));
        }
    }
    
    public override OptionBehaviour CreateOption()
    {
        StringOption option = Object.Instantiate(CustomOptionManager.StringOptionPrefab, CustomOptionManager.StringOptionPrefab.transform.parent);
        option.name = $"{Title}-Option";
        option.Title = CustomStringName.CreateAndRegister(Title);
        option.TitleText.text = Title;
        option.Value = Value;
        option.Values = StringValues.ToArray();
        option.OnValueChanged = new Action<OptionBehaviour>(optionBehaviour => SetValue(optionBehaviour.GetInt()));

        Option = option;
        
        return option;
    }
    
    public CustomStringOption(string id, string title, params string[] values) : base(title)
    {
        Assembly = Assembly.GetCallingAssembly();
        Id = $"{Assembly.GetName().Name}.StringOption.{id}";
        try
        {
            _configEntry = PeasmodPlugin.ConfigFile.Bind("Options", Id, 0);
        }
        catch (Exception e)
        {
            PeasmodPlugin.Logger.LogError($"Error while loading the option \"{title}\": {e.Message}");
        }
            
        Value = _configEntry?.Value ?? 0;
        StringValues = values.ToList().ConvertAll(CustomStringName.CreateAndRegister);
        HudFormat = "{0}: {1}";
            
        CustomOptionManager.CustomOptions.Add(this);
    }
}