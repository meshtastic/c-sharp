using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Meshtastic.Cli.Commands;

public class CannedMessagesCommand : Command
{
    public CannedMessagesCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) :
        base(name, description)
    {
        var operationArg = new Argument<GetSetOperation>("operation", description: "Get or set canned messages");
        operationArg.SetDefaultValue(GetSetOperation.Get);
        AddArgument(operationArg);

        var messagesArg = new Argument<string?>("messages", description: "Pipe | delimited canned message");
        messagesArg.AddValidator(result =>
        {
            if (result.GetValueForArgument(operationArg) == GetSetOperation.Set)
            {
                var messages = result.GetValueForArgument(messagesArg);
                if (messages == null || !messages.Contains('|'))
                    result.ErrorMessage = "Must specify pipe delimited messages";
            }
        });
        messagesArg.SetDefaultValue(null);
        AddArgument(messagesArg);


        this.SetHandler(async (operation, messages, context, commandContext) =>
            {
                if (operation == GetSetOperation.Get)
                {
                    var handler = new GetCannedMessagesCommandHandler(context, commandContext);
                    await handler.Handle();
                }
                else
                {
                    var handler = new SetCannedMessagesCommandHandler(messages!, context, commandContext);
                    await handler.Handle();
                }
            },
            operationArg,
            messagesArg,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
