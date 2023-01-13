using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class TraceRouteMessageFactoryTests
{
    private Fixture fixture;
    private DeviceStateContainer deviceStateContainer;
    private TraceRouteMessageFactory factory;

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
        factory = new TraceRouteMessageFactory(deviceStateContainer);
    }

    [Test]
    public void CreateRouteDiscoveryPacket_Should_ReturnValidAdminMessage()
    {
        factory = new TraceRouteMessageFactory(deviceStateContainer, 100);
        var result = factory.CreateRouteDiscoveryPacket();
        result.Decoded.Portnum.Should().Be(PortNum.TracerouteApp);
    }

    [Test]
    public void CreateRouteDiscoveryPacket_Should_ThrowNotNullException()
    {
        factory = new TraceRouteMessageFactory(deviceStateContainer);
        var action = () => factory.CreateRouteDiscoveryPacket();
        action.Should().Throw<InvalidOperationException>();
    }
}
