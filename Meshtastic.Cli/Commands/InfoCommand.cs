using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;
public class InfoCommand : Command
{
    public InfoCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        this.SetHandler(async (context, outputFormat, logger) =>
            {
                var handler = new InfoCommandHandler(context, outputFormat, logger);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
    }
}
public class InfoCommandHandler : DeviceCommandHandler
{
    public InfoCommandHandler(DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger) { }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container, OutputFormat);
        printer.Print();
        return Task.CompletedTask;
    }
}
