using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class TraceRouteCommandHandler : DeviceCommandHandler
{
    public TraceRouteCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation($"Tracing route to {container.GetNodeDisplayName(Destination!.Value)}...");
        var messageFactory = new TraceRouteMessageFactory(container, Destination);
        var message = messageFactory.CreateRouteDiscoveryPacket();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message),
            (fromDevice, container) =>
            {
                if (fromDevice.ParsedMessage.fromRadio?.Packet?.Decoded?.Portnum == PortNum.TracerouteApp)
                {
                    var routeDiscovery = RouteDiscovery.Parser.ParseFrom(fromDevice.ParsedMessage.fromRadio?.Packet.Decoded.Payload);
                    if (routeDiscovery.Route.Count > 0)
                    {
                        var printer = new ProtobufPrinter(container, OutputFormat);
                        printer.PrintRoute(routeDiscovery.Route);
                    }
                    else
                        Logger.LogWarning("No routes discovered");
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
