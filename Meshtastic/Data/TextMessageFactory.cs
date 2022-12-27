using Google.Protobuf;
using Meshtastic.Protobufs;

namespace Meshtastic.Data;

public class TextMessageFactory
{
    private readonly DeviceStateContainer container;

    public TextMessageFactory(DeviceStateContainer container)
    {
        this.container = container;
    }

    public MeshPacket GetTextMessagePacket(string message, uint channel = 0, uint? to = null, uint? dest = null)
    {
        return new MeshPacket()
        {
            Channel = channel,
            WantAck = false,
            To = to ?? container.MyNodeInfo.MyNodeNum,
            Id = (uint)Math.Floor(Random.Shared.Next() * 1e9),
            HopLimit = container.GetHopLimitOrDefault(),
            Decoded = new Protobufs.Data()
            {
                Dest = dest ?? container.MyNodeInfo.MyNodeNum,
                Portnum = PortNum.TextMessageApp,
                Payload = ByteString.CopyFromUtf8(message),
                WantResponse = true,
            },
        };
    }
}