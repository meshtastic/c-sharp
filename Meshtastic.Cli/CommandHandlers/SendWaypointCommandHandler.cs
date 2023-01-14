using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Meshtastic.Cli.CommandHandlers;

public class SendWaypointCommandHandler : DeviceCommandHandler
{
    private readonly decimal latitude;
    private readonly decimal longitude;
    private readonly string name;
    private readonly string description;
    private readonly string icon;
    private readonly bool locked;
    private readonly decimal divisor = new(1e-7);

    public SendWaypointCommandHandler(decimal latitude,
        decimal longitude,
        string name,
        string description,
        string icon,
        bool locked,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.name = name;
        this.description = description;
        this.icon = icon;
        this.locked = locked;
    }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var factory = new WaypointMessageFactory(container, Destination);

        var message = factory.CreateWaypointPacket(new Waypoint()
        {
            Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
            LatitudeI = latitude != 0 ? decimal.ToInt32(latitude / divisor) : 0,
            LongitudeI = longitude != 0 ? decimal.ToInt32(longitude / divisor) : 0,
            Icon = BitConverter.ToUInt32(Encoding.UTF8.GetBytes(icon)),
            Name = name,
            Description = description ?? String.Empty,
            Locked = locked,
        });
        Logger.LogInformation($"Sending waypoint to device...");
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message),
            (fromDevice, container) =>
            {
                if (fromDevice.ParsedMessage.fromRadio?.Packet?.Decoded.Portnum == PortNum.RoutingApp &&
                   fromDevice.ParsedMessage.fromRadio?.Packet?.Priority == MeshPacket.Types.Priority.Ack)
                {
                    var routingResult = Routing.Parser.ParseFrom(fromDevice.ParsedMessage.fromRadio?.Packet.Decoded.Payload);
                    if (routingResult.ErrorReason == Routing.Types.Error.None)
                        Logger.LogInformation("Acknowledged");
                    else
                        Logger.LogInformation($"Message delivery failed due to reason: {routingResult.ErrorReason}");

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            });
    }
}
