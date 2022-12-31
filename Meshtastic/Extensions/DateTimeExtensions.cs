namespace Meshtastic.Extensions;

public static class DateTimeExtensions
{
    public static uint GetUnixTimestamp(this DateTime dateTime)
    {
        return Convert.ToUInt32(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }
}
