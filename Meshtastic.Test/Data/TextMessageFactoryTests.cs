using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class TextMessageFactoryTests
{
    private Fixture fixture;
    private DeviceStateContainer deviceStateContainer;
    private TextMessageFactory factory;

    [SetUp]
    public void Setup()
    {
        fixture = new Fixture();
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
    public void CreateBeginEditSettingsMessage_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateTextMessagePacket("Text");
        result.Decoded.Portnum.Should().Be(PortNum.TextMessageApp);
    }
}
