using Google.Protobuf;
using Meshtastic.Protobufs;
using Meshtastic.Extensions;

namespace Meshtastic.Data.MessageFactories;

public class TelemetryMessageFactory
{
    private readonly DeviceStateContainer container;
    private readonly uint? dest;

    public TelemetryMessageFactory(DeviceStateContainer container, uint? dest = null)
    {
        this.container = container;
        this.dest = dest;
    }

    public MeshPacket CreateTelemetryPacket(uint channel = 0)
    {
        var telemetry = container!.FromRadioMessageLog
            .Where(fromRadio => fromRadio.Packet.From == container.MyNodeInfo.MyNodeNum)
            .First(fromRadio => fromRadio.GetPayload<Telemetry>()?.DeviceMetrics != null)?.GetPayload<Telemetry>()?.DeviceMetrics;
        return new MeshPacket()
        {
            Channel = channel,
            WantAck = true,
            To = dest ?? 0,
            Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
            HopLimit = container?.GetHopLimitOrDefault() ?? 3,
            Decoded = new Protobufs.Data()
            {
                Portnum = PortNum.TelemetryApp,
                Payload = new Telemetry() 
                { 
                    DeviceMetrics = telemetry ?? new DeviceMetrics()
                }.ToByteString(),
                WantResponse = true,
            },
        };
    }
}