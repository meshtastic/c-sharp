using Google.Protobuf;
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
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        this.SetHandler(async (context, outputFormat, logger) =>
            {
                var handler = new MetadataCommandHandler(context, outputFormat, logger);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
    }
}
public class MetadataCommandHandler : DeviceCommandHandler
{
    public MetadataCommandHandler(DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting device metadata...");
        var adminMessageFactory = new AdminMessageFactory(container);
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
