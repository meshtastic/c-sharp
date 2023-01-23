using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class TraceRouteCommandHandler : DeviceCommandHandler
{
    public TraceRouteCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation($"Tracing route to {container.GetNodeDisplayName(Destination!.Value)}...");
        var messageFactory = new TraceRouteMessageFactory(container, Destination);
        var message = messageFactory.CreateRouteDiscoveryPacket();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message),
            (fromRadio, container) =>
            {
                var routeDiscovery = fromRadio.GetMessage<RouteDiscovery>();
                if (routeDiscovery != null)
                {
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
