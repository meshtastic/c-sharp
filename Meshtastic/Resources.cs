namespace Meshtastic;
public static class Resources
{
    public const int DEFAULT_BAUD_RATE = 115200;
    public const int DEFAULT_TCP_PORT = 4403;

    public const int DEFAULT_READ_TIMEOUT = 15000;
    public const int MAX_TO_FROM_RADIO_LENGTH = 512;

    public static readonly byte[] DEFAULT_PSK = [0xd4, 0xf1, 0xbb, 0x3a, 0x20, 0x29, 0x07, 0x59, 0xf0, 0xbc, 0xff, 0xab, 0xcf, 0x4e, 0x69, 0x1];
}