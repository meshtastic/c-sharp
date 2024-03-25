using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class FixedPositionCommandHandler(decimal latitude,
    decimal longitude,
    int altitude,
    bool clear,
    DeviceConnectionContext context,
    CommandContext commandContext) : DeviceCommandHandler(context, commandContext)
{
    private readonly decimal latitude = latitude;
    private readonly decimal longitude = longitude;
    private readonly int altitude = altitude;
    private readonly bool clear = clear;
    private readonly decimal divisor = new(1e-7);

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);

        var position = new Position()
        {
            LatitudeI = latitude != 0 ? decimal.ToInt32(latitude / divisor) : 0,
            LongitudeI = longitude != 0 ? decimal.ToInt32(longitude / divisor) : 0,
            Altitude = altitude,
            Time = DateTime.Now.GetUnixTimestamp(),
            Timestamp = DateTime.Now.GetUnixTimestamp(),
        };

        var adminMessage = clear ? adminMessageFactory.RemovedFixedPositionMessage() : adminMessageFactory.CreateFixedPositionMessage(position);
        Logger.LogInformation($"Setting fixed position...");

        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), 
            (fromRadio, container) =>
            {
                return Task.FromResult(fromRadio.GetPayload<Routing>() != null);
            });
    }
}
