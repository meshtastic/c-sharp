using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Test.Data;

public class FromDeviceMessageTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void FromDeviceMessage_SwallowsException_Given_BadPayload()
    {
        var fixture = new Fixture();
        var action = () =>
        {
            var derp = new FromDeviceMessage(fixture.Create<ILogger>());
            derp.ParsedFromRadio(BitConverter.GetBytes(123456));
        };
        action.Should().NotThrow<InvalidProtocolBufferException>();
    }
}
