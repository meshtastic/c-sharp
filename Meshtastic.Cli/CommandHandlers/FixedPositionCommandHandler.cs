using Meshtastic.Data;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class FixedPositionCommandHandler : DeviceCommandHandler
{
    private readonly decimal latitude;
    private readonly decimal longitude;
    private readonly int altitude;
    private readonly decimal divisor = new(1e-7);

    public FixedPositionCommandHandler(decimal latitude,
        decimal longitude,
        int altitude,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
    }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var positionMessageFactory = new PositionMessageFactory(container);

        await BeginEditSettings(adminMessageFactory);

        var positionConfig = container.LocalConfig.Position;
        positionConfig.FixedPosition = true;
        var adminMessage = adminMessageFactory.CreateSetConfigMessage(positionConfig);
        Logger.LogInformation($"Setting Position.FixedPosition to True...");

        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AdminMessageResponseReceived);
        var positionMessage = positionMessageFactory.GetNewPositionPacket(new Position()
        {
            LatitudeI = latitude != 0 ? decimal.ToInt32(latitude / divisor) : 0,
            LongitudeI = longitude != 0 ? decimal.ToInt32(longitude / divisor) : 0,
            Altitude = altitude,
            Time = DateTime.Now.GetUnixTimestamp(),
            Timestamp = DateTime.Now.GetUnixTimestamp(),
        });
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(positionMessage), AnyResponseReceived);
        Logger.LogInformation($"Sending position to device...");

        await CommitEditSettings(adminMessageFactory);
    }
}
