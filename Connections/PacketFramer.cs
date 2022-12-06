namespace Meshtastic.Connections;

public static class PacketFramer
{
    public static byte[] CreatePacket(byte[] data) => 
        Resources.PACKET_FRAME
            .Concat(new byte[] { (byte)data.Length })
            .Concat(data)
            .ToArray();
}