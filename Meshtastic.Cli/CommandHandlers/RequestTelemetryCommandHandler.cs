using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class RequestTelemetryCommandHandler : DeviceCommandHandler
{
    public RequestTelemetryCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
    }

    public async Task<DeviceStateContainer> Handle()
    {
        if (Destination is null)
            throw new ApplicationException("Destination must be specified to request telemetry");

        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var telemetryMessageFactory = new TelemetryMessageFactory(container, Destination);
        var message = telemetryMessageFactory.CreateTelemetryPacket();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message), async (packet, container) =>
        {
            // if (packet?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Packet &&
            //     packet.MeshPacket is not null)
            Logger.LogInformation($"Received telemetry from device: {packet}");
            return await Task.FromResult(true);
        });
    }
}
