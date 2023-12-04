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

            public Data(CustomOption option, object value)
            {
                Option = option;
                Value = value;
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
            }
        }

        public override Data Read(MessageReader reader)
        {
            var id = reader.ReadString();
            var option = CustomOptionManager.CustomOptions.Find(_option => _option.Id == id);
            object value = null;
            
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
            }
            
            return new Data(option, value);
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
            }
        }
    }