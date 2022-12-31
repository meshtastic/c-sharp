using Microsoft.Extensions.Logging;
using Meshtastic.Cli.Binders;
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
