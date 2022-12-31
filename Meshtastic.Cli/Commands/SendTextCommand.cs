using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class SendTextCommand : Command
{
    public SendTextCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var messageArg = new Argument<string>("message", description: "Text message contents");
        messageArg.AddValidator(result =>
        {
            if (String.IsNullOrWhiteSpace(result.GetValueForArgument(messageArg)))
                result.ErrorMessage = "Must specify a message";
        });
        AddArgument(messageArg);

        this.SetHandler(async (message, context, commandContext) =>
            {
                var handler = new SendTextCommandHandler(message, context, commandContext);
                await handler.Handle();
            },
            messageArg,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
