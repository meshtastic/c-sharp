namespace Meshtastic.Connections;

public static class PacketFraming
{
    public static byte[] PACKET_FRAME_START => new byte[] { 0x94, 0xc3 };
    public static byte[] SERIAL_PREAMBLE => new byte[] 
    { 
        PACKET_FRAME_START[1],
        PACKET_FRAME_START[1],
        PACKET_FRAME_START[1],
        PACKET_FRAME_START[1]
    };

    public const int PACKET_HEADER_LENGTH = 4;
    public static byte[] GetPacketHeader(byte[] data) => 
        new byte[] {
            PACKET_FRAME_START[0],
            PACKET_FRAME_START[1],
            (byte)((data.Length >> 8) & 0xff),
            (byte)(data.Length & 0xff),
        };

    public static byte[] CreatePacket(byte[] data) =>
        GetPacketHeader(data)
        .Concat(data)
        .ToArray();
}
