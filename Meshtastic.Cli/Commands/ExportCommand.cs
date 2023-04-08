using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;
public class ExportCommand : Command
{
    public ExportCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        this.SetHandler(async (context, commandContext) =>
            {
                var handler = new ExportCommandHandler(context, commandContext);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, new Option<uint?>("dest"), new Option<bool>("select-dest")));
    }
}
