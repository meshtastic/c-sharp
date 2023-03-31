namespace Meshtastic.Extensions;

public static class DateTimeExtensions
{
    public static uint GetUnixTimestamp(this DateTime dateTime)
    {
        return Convert.ToUInt32(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }

    public static string AsTimeAgo(this DateTime dateTime)
    {
        TimeSpan timeSpan = DateTime.UtcNow.Subtract(dateTime);

        return timeSpan.TotalSeconds switch
        {
            <= 60 => $"{timeSpan.Seconds} seconds ago",

            _ => timeSpan.TotalMinutes switch
            {
                <= 1 => "A minute ago",
                < 60 => $"{timeSpan.Minutes} minutes ago",
                _ => timeSpan.TotalHours switch
                {
                    <= 1 => "A hour ago",
                    < 24 => $"{timeSpan.Hours} hours ago",
                    _ => timeSpan.TotalDays switch
                    {
                        <= 1 => "yesterday",
                        <= 30 => $"{timeSpan.Days} days ago",

                        <= 60 => "A month ago",
                        < 365 => $"{timeSpan.Days / 30} months ago",

                        <= 365 * 2 => "A year ago",
                        _ => $"{timeSpan.Days / 365} years ago"
                    }
                }
            }
        };
    }
}
