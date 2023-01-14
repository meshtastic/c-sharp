using Google.Protobuf;

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
        var action = () =>
        {
            _ = new FromDeviceMessage(BitConverter.GetBytes(123242));
        };
        action.Should().NotThrow<InvalidProtocolBufferException>();
    }
}
