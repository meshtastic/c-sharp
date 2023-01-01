using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class MetadataCommandHandler : DeviceCommandHandler
{
    public MetadataCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting device metadata...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateGetMetadataMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            (fromDevice, container) =>
            {
                if (fromDevice?.ParsedMessage.adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.GetDeviceMetadataResponse)
                {
                    var printer = new ProtobufPrinter(container, OutputFormat);
                    printer.PrintMetadata(fromDevice.ParsedMessage.adminMessage!.GetDeviceMetadataResponse);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
