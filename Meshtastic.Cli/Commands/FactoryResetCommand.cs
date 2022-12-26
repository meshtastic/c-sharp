using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class FactoryResetCommand : Command
{
    public FactoryResetCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        this.SetHandler(async (context, outputFormat, logger) =>
            {
                var handler = new FactoryResetCommandHandler(context, outputFormat, logger);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
    }
}
public class FactoryResetCommandHandler : DeviceCommandHandler
{
    public FactoryResetCommandHandler(DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Factory reseting device...");
        var adminMessageFactory = new AdminMessageFactory(container);
        var adminMessage = adminMessageFactory.CreateFactoryResetMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), 
            (fromDevice, container) =>
            {
                return Task.FromResult(fromDevice != null);
            });
    }
}
