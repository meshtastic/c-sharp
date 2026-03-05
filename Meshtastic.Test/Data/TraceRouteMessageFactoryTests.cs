using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class TraceRouteMessageFactoryTests
{
    private DeviceStateContainer deviceStateContainer;
    private TraceRouteMessageFactory factory;

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
        factory = new TraceRouteMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreateRouteDiscoveryPacket_Should_ReturnValidAdminMessage()
    {
        factory = new TraceRouteMessageFactory(deviceStateContainer, 100);
        var result = factory.CreateRouteDiscoveryPacket();
        result.Decoded.Portnum.ShouldBe(PortNum.TracerouteApp);
    }

    [Test]
    public void CreateRouteDiscoveryPacket_Should_ThrowNotNullException()
    {
        factory = new TraceRouteMessageFactory(deviceStateContainer);
        var action = () => factory.CreateRouteDiscoveryPacket();
        action.ShouldThrow<InvalidOperationException>();
    }
}
