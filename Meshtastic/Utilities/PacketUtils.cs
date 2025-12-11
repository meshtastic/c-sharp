namespace Meshtastic.Utilities;

public static class PacketUtils
{
    /// <summary>
    /// Generate next random packet id
    /// </summary>
    public static uint GenerateRandomPacketId()
    {
        return (uint)Random.Shared.NextInt64(0, (long)uint.MaxValue + 1);
    }
}
