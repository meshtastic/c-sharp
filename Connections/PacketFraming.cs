namespace Meshtastic.Connections;

public static class PacketFraming
{
    public static byte[] GetPacketHeader(byte[] data) => 
        new byte[] {
            Resources.PACKET_FRAME_START[0],
            Resources.PACKET_FRAME_START[1],
            (byte)((data.Length >> 8) & 0xff),
            (byte)(data.Length & 0xff),
        };

    public static byte[] CreatePacket(byte[] data) =>
        GetPacketHeader(data)
        .Concat(data)
        .ToArray();
}
