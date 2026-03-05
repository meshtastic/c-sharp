using Meshtastic.Extensions;

namespace Meshtastic.Test.Extensions;

[TestFixture]
public class DateTimeExtensionsTests
{
    [Test]
    public void GetUnixTimestamp_Should_ReturnEpoch()
    {
        var zeroEpoch = new DateTime(1970, 1, 1);
        zeroEpoch.GetUnixTimestamp().ShouldBe((uint)0);
    }
}
