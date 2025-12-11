using Google.Protobuf;
using Meshtastic.Protobufs;

namespace Meshtastic.Data.MessageFactories;

public class TraceRouteMessageFactory
{
    private readonly DeviceStateContainer container;
    private readonly uint? dest;

    public TraceRouteMessageFactory(DeviceStateContainer container, uint? dest = null)
    {
        this.container = container;
        this.dest = dest;
    }

    public MeshPacket CreateRouteDiscoveryPacket(uint channel = 0)
    {
        var p1 = (uint)Random.Shared.Next();
        var p2 = (uint)Random.Shared.Next();
        var id = unchecked(p1 + p2);
        return new MeshPacket()
        {
            Channel = channel,
            To = dest!.Value,
            Id = id,
            HopLimit = container?.GetHopLimitOrDefault() ?? 3,
            Priority = MeshPacket.Types.Priority.Reliable,
            Decoded = new Protobufs.Data()
            {
                WantResponse = true,
                Portnum = PortNum.TracerouteApp,
                Payload = new RouteDiscovery().ToByteString(), // Traceroute just wants an empty bytestring
            },
        };
    }
}