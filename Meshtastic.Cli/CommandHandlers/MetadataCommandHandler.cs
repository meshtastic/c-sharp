using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class MetadataCommandHandler : DeviceCommandHandler
{
    public MetadataCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting device metadata...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateGetMetadataMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            (fromRadio, container) =>
            {
                var adminMessage = fromRadio.GetPayload<AdminMessage>();
                if (adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.GetDeviceMetadataResponse)
                {
                    var printer = new ProtobufPrinter(container, OutputFormat);
                    printer.PrintMetadata(adminMessage!.GetDeviceMetadataResponse);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
