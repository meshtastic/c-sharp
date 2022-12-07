public static class Resources
{
    public const int DEFAULT_BAUD_RATE = 115200;
    public const int DEFAULT_READ_TIMEOUT = 15000;
    public const int MAX_TO_FROM_RADIO_LENGTH = 512;

    public static byte[] PACKET_FRAME_START = new byte[] { 0x94, 0xc3 };
    public static byte[] SERIAL_PREAMBLE = new byte[] 
    { 
        Resources.PACKET_FRAME_START[1],
        Resources.PACKET_FRAME_START[1],
        Resources.PACKET_FRAME_START[1],
        Resources.PACKET_FRAME_START[1]
    };
    public const int PACKET_HEADER_LENGTH = 4;
}