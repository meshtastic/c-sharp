using Google.Protobuf;
using Meshtastic.Cli.Serialization;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Meshtastic.Cli.CommandHandlers;

public class ExportCommandHandler : DeviceCommandHandler
{
    private ISerializer yamlSerializer = new SerializerBuilder()
        .IgnoreFields()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeInspector(inner => new FilteredTypeInspector(inner))
        .Build();
    
    public ExportCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var deviceProfile = new DeviceProfile()
        {
            Config = container.LocalConfig,
            ModuleConfig = container.LocalModuleConfig
        };

        var me = container.GetDeviceNodeInfo();
        if (me?.User != null)
        {
            deviceProfile.LongName = me.User.LongName;
            deviceProfile.ShortName = me.User.ShortName;
        }

        deviceProfile.ChannelUrl = container.GetChannelUrl();
        
        AnsiConsole.WriteLine(yamlSerializer.Serialize(deviceProfile));

        return Task.CompletedTask;
    }
}
