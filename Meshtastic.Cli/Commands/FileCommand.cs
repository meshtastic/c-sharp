using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class FileCommand : Command
{
    public FileCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        var pathArgument = new Argument<string>("path", "The type of url operation");
        this.AddArgument(pathArgument);

        this.SetHandler(async (path, context, commandContext) =>
            {
                var handler = new FileCommandHandler(path, context, commandContext);
                await handler.Handle();
            },
            pathArgument,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, new Option<uint?>("dest") { }, new Option<bool>("select-dest") { }));
    }
}
