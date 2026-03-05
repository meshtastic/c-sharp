using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class WaypointMessageFactoryTests
{
    private DeviceStateContainer deviceStateContainer;
    private WaypointMessageFactory factory;

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
        factory = new WaypointMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreateWaypointPacket_Should_ReturnValidAdminMessage()
    {
        var result = factory.CreateWaypointPacket(new Waypoint());
        result.Decoded.Portnum.ShouldBe(PortNum.WaypointApp);
    }
}
