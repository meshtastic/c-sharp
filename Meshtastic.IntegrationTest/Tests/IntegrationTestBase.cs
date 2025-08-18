using Meshtastic.Discovery;

namespace Meshtastic.IntegrationTest.Tests;

public class IntegrationTestBase
{
    protected static IEnumerable<MeshtasticDevice> GetDevicesUnderTest() => GlobalSetup.DiscoveredDevices;
}
