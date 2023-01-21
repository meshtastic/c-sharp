using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class TextMessageFactoryTests
{
    private DeviceStateContainer deviceStateContainer;
    private TextMessageFactory factory;

    [SetUp]
    public void Setup()
    {
        deviceStateContainer = new DeviceStateContainer();
        deviceStateContainer.MyNodeInfo.MyNodeNum = 100;
        deviceStateContainer.LocalConfig = new LocalConfig
        {
            Lora = new LoRaConfig()
            {
                HopLimit = 3,
            }
        };
        factory = new TextMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreateTextMessagePacket_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateTextMessagePacket("Text");
        result.Decoded.Portnum.Should().Be(PortNum.TextMessageApp);
    }
}
