using Google.Protobuf;
using Meshtastic.Protobufs;

namespace Meshtastic.Data.MessageFactories;

public class PositionMessageFactory
{
    private readonly DeviceStateContainer container;
    private readonly uint? dest;

    public PositionMessageFactory(DeviceStateContainer container, uint? dest = null)
    {
        this.container = container;
        this.dest = dest;
    }

    public MeshPacket CreatePositionPacket(Position message, uint channel = 0)
    {
        return new MeshPacket()
        {
            Channel = channel,
            WantAck = true,
            To = dest ?? container.MyNodeInfo.MyNodeNum,
            Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
            HopLimit = container.GetHopLimitOrDefault(),
            Decoded = new Protobufs.Data()
            {
                Portnum = PortNum.PositionApp,
                Payload = message.ToByteString(),
            },
        };
    }
}