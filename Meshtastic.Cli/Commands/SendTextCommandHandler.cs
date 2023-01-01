using Meshtastic.Data;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

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
        Logger.LogInformation($"Sending text messagee...");

        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(textMessage),
             (fromDevice, container) =>
             {
                 if (fromDevice.ParsedMessage.fromRadio?.Packet.Decoded.Portnum == PortNum.RoutingApp &&
                    fromDevice.ParsedMessage.fromRadio?.Packet.Priority == MeshPacket.Types.Priority.Ack)
                 {
                     var routingResult = Routing.Parser.ParseFrom(fromDevice.ParsedMessage.fromRadio?.Packet.Decoded.Payload);
                     if (routingResult.ErrorReason == Routing.Types.Error.None)
                         Logger.LogInformation("Acknowledged");
                     else
                         Logger.LogInformation($"Message delivery failed due to reason: {routingResult.ErrorReason}");

                     return Task.FromResult(true);
                 }

                 return Task.FromResult(fromDevice.ParsedMessage.fromRadio != null);
             });
    }
}
