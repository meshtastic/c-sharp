using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Utilities;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Cli.Commands;

[ExcludeFromCodeCoverage(Justification = "Hardware access")]
public class UpdateCommand : Command
{
    public UpdateCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) :
        base(name, description)
    {
        this.SetHandler(async (context, commandContext) =>
            {
                var handler = new UpdateCommandHandler(new FirmwarePackageService(), new ReleaseZipService(), context, commandContext);
                await handler.Handle();
            },
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, new Option<uint?>("dest") { }, new Option<bool>("select-dest") { }));
    }
}
