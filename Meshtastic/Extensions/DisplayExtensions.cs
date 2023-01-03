using Meshtastic.Protobufs;

namespace Meshtastic.Extensions
{
    public static class DisplayExtensions
    {
        public static string ToDisplayString(this Position position)
        {
            var display = "Not available";
            if (position == null || (position.LatitudeI == 0 && position.LongitudeI == 0))
                return display;

            return $"{position.LatitudeI * 1e-7}, {position.LongitudeI * 1e-7}";
        }
    }
}
