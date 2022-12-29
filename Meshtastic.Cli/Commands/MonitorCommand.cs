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
        this.SetHandler(async (context, commandContext) =>
        {
            var handler = new MonitorCommandHandler(context, commandContext);
            await handler.Handle();
        },
        new DeviceConnectionBinder(port, host),
        new CommandContextBinder(log, output, new Option<uint?>("dest") { }));
    }
}

public class MonitorCommandHandler : DeviceCommandHandler
{
    public MonitorCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) { }
    public async Task Handle()
    {
        await Connection.Monitor();
    }
}
