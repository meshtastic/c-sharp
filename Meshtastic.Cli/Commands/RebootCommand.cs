using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class RebootCommand : Command
{
    public RebootCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var otaOption = new Option<bool>("ota", "Reboot into OTA update mode");
        otaOption.SetDefaultValue(false);

        var secondsArgument = new Argument<int>("seconds", "Number of seconds until reboot");
        secondsArgument.SetDefaultValue(5);

        this.SetHandler(async (isOtaMode, seconds, context, commandContext) =>
            {
                var handler = new RebootCommandHandler(isOtaMode, seconds, context, commandContext);
                await handler.Handle();
            },
            otaOption,
            secondsArgument,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
        this.AddOption(otaOption);
        this.AddArgument(secondsArgument);
    }
}