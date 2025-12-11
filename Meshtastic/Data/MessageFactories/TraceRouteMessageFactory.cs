using Google.Protobuf;
using Meshtastic.Protobufs;
using Meshtastic.Utilities;

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
        return new MeshPacket()
        {
            Channel = channel,
            To = dest!.Value,
            Id = PacketUtils.GenerateRandomPacketId(),
            HopLimit = container.GetHopLimitOrDefault(),
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