using Spectre.Console;

namespace Meshtastic;
public static class Resources
{
    public static Color MESHTASTIC_GREEN => new(103, 234, 148);
    public const int DEFAULT_BAUD_RATE = 115200;
    public const int DEFAULT_TCP_PORT = 4403;

    public const int DEFAULT_READ_TIMEOUT = 15000;
    public const int MAX_TO_FROM_RADIO_LENGTH = 512;
}