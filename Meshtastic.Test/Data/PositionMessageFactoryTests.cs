using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class PositionMessageFactoryTests
{
    private Fixture fixture;
    private DeviceStateContainer deviceStateContainer;
    private PositionMessageFactory factory;

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
        factory = new PositionMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreatePositionPacket_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreatePositionPacket(new Position());
        result.Decoded.Portnum.Should().Be(PortNum.PositionApp);
    }
}
