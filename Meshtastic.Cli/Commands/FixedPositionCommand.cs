using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Meshtastic.Protobufs;
using Meshtastic.Extensions;

namespace Meshtastic.Cli.Commands;

public class FixedPositionCommand : Command
{
    public FixedPositionCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var latArg = new Argument<decimal>("lat", description: "Latitude of the node (decimal format)");
        latArg.AddValidator(result => 
        {
            if (Math.Abs(result.GetValueForArgument(latArg)) > 90)
                result.ErrorMessage = "Invalid latitude";
        });
        AddArgument(latArg);

        var lonArg = new Argument<decimal>("lon", description: "Longitude of the node (decimal format)");
        lonArg.AddValidator(result =>
        {
            if (Math.Abs(result.GetValueForArgument(lonArg)) > 180)
                result.ErrorMessage = "Invalid latitude";
        });
        AddArgument(lonArg);

        var altArg = new Argument<int>("altitude", description: "Altitude of the node (meters)");
        altArg.SetDefaultValue(0);
        AddArgument(altArg);

        this.SetHandler(async (lat, lon, alt, context, commandContext) =>
            {
                var handler = new FixedPositionCommandHandler(lat, lon, alt, context, commandContext);
                await handler.Handle();
            },
            latArg, 
            lonArg, 
            altArg,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
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
            LatitudeI = latitude != 0 ? Decimal.ToInt32(latitude / divisor) : 0,
            LongitudeI = longitude != 0 ? Decimal.ToInt32(longitude / divisor) : 0,
            Altitude = altitude,
            Time = DateTime.Now.GetUnixTimestamp(),
            Timestamp = DateTime.Now.GetUnixTimestamp(),
        });
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(positionMessage), AnyResponseReceived);
        Logger.LogInformation($"Sending position to device...");

        await CommitEditSettings(adminMessageFactory);
    }
}
