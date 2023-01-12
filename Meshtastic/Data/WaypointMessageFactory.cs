using Google.Protobuf;
using Meshtastic.Protobufs;

namespace Meshtastic.Data;

public class WaypointMessageFactory
{
    private readonly DeviceStateContainer container;
    private readonly uint? dest;

    public WaypointMessageFactory(DeviceStateContainer container, uint? dest = null)
    {
        this.container = container;
        this.dest = dest;
    }

    public MeshPacket CreateWaypointPacket(Waypoint waypoint, uint channel = 0)
    {
        return new MeshPacket()
        {
            Channel = channel,
            WantAck = true,
            To = dest ?? 0xffffffff, // Default to broadcast
            Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
            HopLimit = container.GetHopLimitOrDefault(),
            Decoded = new Protobufs.Data()
            {
                Portnum = PortNum.WaypointApp,
                Payload = waypoint.ToByteString(),
            },
        };
    }
}