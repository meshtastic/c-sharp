using Meshtastic.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Extensions
{
    [TestFixture]
    public class DisplayExtensionsTests
    {
        [Test]
        public void ToDisplayString_Should_ReturnNotAvailable_GivenNullPosition()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Position position = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            var result = position.ToDisplayString();
#pragma warning restore CS8604 // Possible null reference argument.
            result.Should().Be("Not available");
        }

        [Test]
        public void ToDisplayString_Should_ReturnNotAvailable_GivenZeroLatAndLongPosition()
        {
            Position position = new()
            {
                LatitudeI = 0,
                LongitudeI = 0, 
            };
            var result = position.ToDisplayString();
            result.Should().Be("Not available");
        }

        [Test]
        public void ToDisplayString_Should_ReturnFormattedCoordinates_GivenValidLatAndLongPosition()
        {
            Position position = new()
            {
                LatitudeI = 341234567,
                LongitudeI = -921234567,
            };
            var result = position.ToDisplayString();
            result.Should().Be("34.1234567, -92.1234567");
        }
    }
}
