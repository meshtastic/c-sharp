using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class MetadataCommand : Command
{
    public MetadataCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : 
        base(name, description)
    {
        this.SetHandler(async (context, commandContext) =>
            {
                var handler = new MetadataCommandHandler(context, commandContext);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
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
