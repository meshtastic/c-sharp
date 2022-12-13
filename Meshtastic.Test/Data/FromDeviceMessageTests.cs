using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Data;

public class FromDeviceMessageTests
{
    [SetUp]
    public void Setup()
    {
    }

    //[Test]
    //public void FromDeviceMessage_FallsBackToAdminMessage_Given_AdminMessagePayload()
    //{
    //    var packet = new AdminMessageFactory(new DeviceStateContainer()).CreateBeginEditSettingsMessage();
    //    var fromDeviceMessage = new FromDeviceMessage(packet.ToByteArray());
    //    var result = fromDeviceMessage.ParsedMessage;

    //    result.adminMessage!.BeginEditSettings.Should().BeTrue();
    //}

    [Test]
    public void FromDeviceMessage_SwallowsException_Given_BadPayload()
    {
        var action = () =>
        {
            new FromDeviceMessage(BitConverter.GetBytes(123242));
        };
        action.Should().NotThrow<InvalidProtocolBufferException>();
    }
}
