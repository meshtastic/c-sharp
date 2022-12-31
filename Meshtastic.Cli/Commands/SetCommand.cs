using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class SetCommand : Command
{
    public SetCommand(string name, string description, Option<IEnumerable<string>> settings, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) :
        base(name, description)
    {
        this.SetHandler(async (settings, context, commandContext) =>
            {
                var handler = new SetCommandHandler(settings, context, commandContext);
                await handler.Handle();
            },
            settings,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
        this.AddOption(settings);
    }
}
