using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Commands;

public class MetadataCommand : Command
{
    public MetadataCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var metadataCommandHandler = new MetadataCommandHandler();
        this.SetHandler(metadataCommandHandler.Handle,
            new ConnectionBinder(port, host),
            new LoggingBinder());
    }
}
public class MetadataCommandHandler : DeviceCommandHandler
{
    public async Task Handle(DeviceConnectionContext context, ILogger logger)
    {
        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
            var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        AnsiConsole.WriteLine("Getting device metadata...");
        var adminMessageFactory = new AdminMessageFactory(container);
        var adminMessage = adminMessageFactory.CreateGetMetadataMessage();
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage).ToByteArray(), 
            (fromDevice, container) =>
            {
                if (fromDevice != null && 
                    fromDevice.ParsedMessage.adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.GetDeviceMetadataResponse) 
                {
                    var printer = new ProtobufPrinter(container);
                    printer.PrintMetadata(fromDevice.ParsedMessage.adminMessage!.GetDeviceMetadataResponse);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
