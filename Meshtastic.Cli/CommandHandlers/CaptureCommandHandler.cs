using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Meshtastic.Extensions;

namespace Meshtastic.Cli.CommandHandlers;

public class CaptureCommandHandler : DeviceCommandHandler
{
    public CaptureCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        await Connection.ReadFromRadio((fromRadio, container) =>
        {
            if (fromRadio!.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Packet) {
                var fromRadioDecoded = new FromRadioDecoded(fromRadio) 
                {
                    PortNum = fromRadio.Packet?.Decoded?.Portnum,
                    PayloadSize = fromRadio.Packet?.Decoded?.Payload.Length ?? 0,
                    ReceivedAt = DateTime.Now,
                };
                if (fromRadio.GetPayload<AdminMessage>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<AdminMessage>();
                else if (fromRadio.GetPayload<XModem>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<XModem>();
                else if (fromRadio.GetPayload<NodeInfo>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<NodeInfo>();
                else if (fromRadio.GetPayload<Position>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<Position>();
                else if (fromRadio.GetPayload<Waypoint>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<Waypoint>();
                else if (fromRadio.GetPayload<Telemetry>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<Telemetry>();
                else if (fromRadio.GetPayload<Routing>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<Routing>();
                else if (fromRadio.GetPayload<RouteDiscovery>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<RouteDiscovery>();
                else if (fromRadio.GetPayload<string>() != null)
                    fromRadioDecoded.DecodedPayload = fromRadio.GetPayload<string>();
            }
                    
            return Task.FromResult(false);
        });
    }
}

public class FromRadioDecoded 
{
    public FromRadioDecoded(FromRadio fromRadio)
    {
        Packet = fromRadio;
    }

    public FromRadio Packet { get; set; }
    public object? DecodedPayload { get; set; }
    public PortNum? PortNum { get; set; }
    public int PayloadSize { get; set; }
    public int TotalSize { get; set; }
    public DateTime ReceivedAt { get; set; }
}