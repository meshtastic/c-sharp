using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class ResetNodeDbCommand : Command
{
    public ResetNodeDbCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) :
        base(name, description)
    {
        this.SetHandler(async (context, commandContext) =>
        {
            var handler = new ResetNodeDbCommandHandler(context, commandContext);
            await handler.Handle();
        },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
