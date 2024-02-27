using System.Linq;
using Hazel;
using Peasmod4.API.UI.Options;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace Peasmod4.API.Networking;

[RegisterCustomRpc((uint) CustomRpcCalls.UpdateSetting)]
    public class RpcUpdateSetting : PlayerCustomRpc<PeasmodPlugin, RpcUpdateSetting.Data>
    {
        public RpcUpdateSetting(PeasmodPlugin plugin, uint id) : base(plugin, id)
        {
        }

        public readonly struct Data
        {
            public readonly CustomOption Option;
            public readonly object Value;
            public readonly object Value2;

            public Data(CustomOption option, object value, object value2 = null)
            {
                Option = option;
                Value = value;
                Value2 = value2;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Option.Id);

            switch (data.Option)
            {
                case CustomToggleOption:
                    writer.Write((bool) data.Value);
                    break;
                case CustomNumberOption:
                    writer.Write((float) data.Value);
                    break;
                case CustomStringOption:
                    writer.Write((int) data.Value);
                    break;
                case CustomRoleOption:
                    writer.Write((int) data.Value);
                    writer.Write((int) data.Value2);
                    break;
            }
        }

        public override Data Read(MessageReader reader)
        {
            var id = reader.ReadString();
            var option = CustomOptionManager.CustomOptions.Find(_option => _option.Id == id) ??
                         CustomOptionManager.CustomRoleOptions.Find(_option => _option.Id == id);
            if (option == null)
            {
                PeasmodPlugin.Logger.LogError("RpcUpdateSetting: Didn't find option with that id");
            }
            object value = null;
            object value2 = null;
            
            switch (option)
            {
                case CustomToggleOption:
                    value = reader.ReadBoolean();
                    break;
                case CustomNumberOption:
                    value = reader.ReadSingle();
                    break;
                case CustomStringOption:
                    value = reader.ReadInt32();
                    break;
                case CustomRoleOption:
                    value = reader.ReadInt32();
                    value2 = reader.ReadInt32();
                    break;
            }
            
            return new Data(option, value, value2);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            switch (data.Option)
            {
                case CustomToggleOption option:
                    option.SetValue((bool) data.Value);
                    break;
                case CustomNumberOption option:
                    option.SetValue((float) data.Value);
                    break;
                case CustomStringOption option:
                    option.SetValue((int) data.Value);
                    break;
                case CustomRoleOption option:
                    option.SetValue((int) data.Value, (int) data.Value2);
                    break;
            }
        }
    }