using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class SendTextCommand : Command
{
    public SendTextCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        var messageArg = new Argument<string>("message", description: "Text message contents");
        messageArg.AddValidator(result =>
        {
            if (String.IsNullOrWhiteSpace(result.GetValueForArgument(messageArg)))
                result.ErrorMessage = "Must specify a message";
        });
        AddArgument(messageArg);

        this.SetHandler(async (message, context, outputFormat, logger) =>
            {
                var handler = new SendTextCommandHandler(message, context, outputFormat, logger);
                await handler.Handle();
            },
            messageArg,
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
    }
}

public class SendTextCommandHandler : DeviceCommandHandler
{
    private readonly string message;

    public SendTextCommandHandler(string message,
        DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger)
    {
        this.message = message;
    }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var textMessageFactory = new TextMessageFactory(container);

        var textMessage = textMessageFactory.GetTextMessagePacket(message);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(textMessage), AnyResponseReceived);
        Logger.LogInformation($"Sending text messagee...");
    }
}
