using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class SendInputEventCommand : Command
{
    public SendInputEventCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var eventCodeArg = new Argument<uint>("event-code", description: "The input event code");
        AddArgument(eventCodeArg);

        var kbCharOption = new Option<uint?>("--kb-char", description: "Keyboard character code");
        AddOption(kbCharOption);

        var touchXOption = new Option<uint?>("--touch-x", description: "Touch X coordinate");
        AddOption(touchXOption);

        var touchYOption = new Option<uint?>("--touch-y", description: "Touch Y coordinate");
        AddOption(touchYOption);

        this.SetHandler(async (eventCode, kbChar, touchX, touchY, context, commandContext) =>
            {
                var handler = new SendInputEventCommandHandler(eventCode, kbChar, touchX, touchY, context, commandContext);
                await handler.Handle();
            },
            eventCodeArg,
            kbCharOption,
            touchXOption,
            touchYOption,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
