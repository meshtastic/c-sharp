using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Meshtastic.Data;
using Meshtastic.Display;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class MonitorCommand : Command
{
    public MonitorCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        this.SetHandler(async (context, outputFormat, logger) =>
        {
            var handler = new MonitorCommandHandler(context, outputFormat, logger);
            await handler.Handle();
        },
        new DeviceConnectionBinder(port, host),
        output,
        new LoggingBinder(log));
    }
}

public class MonitorCommandHandler : DeviceCommandHandler
{
    public MonitorCommandHandler(DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger) { }
    public async Task Handle()
    {
        await Connection.Monitor();
    }
}
