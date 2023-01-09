using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class SetCannedMessagesCommand : Command
{
    public SetCannedMessagesCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) :
        base(name, description)
    {
        var messagesArg = new Argument<string?>("messages", description: "Pipe | delimited canned message");
        messagesArg.AddValidator(result =>
        {
            var messages = result.GetValueForArgument(messagesArg);
            if (messages == null || !messages.Contains('|'))
                result.ErrorMessage = "Must specify pipe delimited messages";
        });
        AddArgument(messagesArg);


        this.SetHandler(async (messages, context, commandContext) =>
            {
                var handler = new SetCannedMessagesCommandHandler(messages!, context, commandContext);
                await handler.Handle();
            },
            messagesArg,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
