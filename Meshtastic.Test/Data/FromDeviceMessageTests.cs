using Google.Protobuf;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Data;
public class FromDeviceMessageTests
{
    private FromDeviceMessage fromDeviceMessage;


    [SetUp]
    public void Setup()
    {
        fromDeviceMessage = new FromDeviceMessage(new FakeLogger());
    }

    [Test]
    public void FromDeviceMessage_SwallowsException_Given_BadPayload()
    {
        var action = () =>
        {
            fromDeviceMessage.ParsedFromRadio(BitConverter.GetBytes(123456));
        };
        action.Should().NotThrow<InvalidProtocolBufferException>();
    }

    [Test]
    public void FromDeviceMessage_GivesResult_Given_ValidFromRadioPayload()
    {
        var fromRadio = new FromRadio()
        {
            Packet = new MeshPacket()
            {
                Decoded = new Protobufs.Data()
                {
                    Portnum = PortNum.AdminApp,
                    Payload = new AdminMessage() { BeginEditSettings = true }.ToByteString(),
                }
            }
        };
        var result = fromDeviceMessage.ParsedFromRadio(fromRadio.ToByteArray());
        result!.GetMessage<AdminMessage>()!.BeginEditSettings.Should().BeTrue();
    }
}
