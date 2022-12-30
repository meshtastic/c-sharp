using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Meshtastic.Display;
using Meshtastic.Protobufs;

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

public class SendTextCommandHandler : DeviceCommandHandler
{
    private readonly string message;

    public SendTextCommandHandler(string message, DeviceConnectionContext context, CommandContext commandContext) : 
        base(context, commandContext)
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
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(textMessage),
             (fromDevice, container) =>
             {
                 if (Destination.HasValue && fromDevice.ParsedMessage.fromRadio?.Packet.From == Destination.Value)
                 {
                     Logger.LogInformation("Acknowledged");
                     return Task.FromResult(true);
                 }

                 return Task.FromResult(fromDevice.ParsedMessage.fromRadio != null);
             });
        Logger.LogInformation($"Sending text messagee...");
    }
}
