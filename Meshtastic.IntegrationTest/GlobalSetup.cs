using Meshtastic.Discovery;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Meshtastic.IntegrationTest
{
    [SetUpFixture]
    public class GlobalSetup
    {
        public static List<MeshtasticDevice> DiscoveredDevices { get; private set; } = new();

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var discovery = new DeviceDiscovery();
            var usbDevices = discovery.DiscoverUsbDevices();

            var seeedL1Tracker = usbDevices.FirstOrDefault(d => d.HwModel == HardwareModel.SeeedWioTrackerL1);
            if (seeedL1Tracker != null)
                DiscoveredDevices.Add(seeedL1Tracker);

            var rak4631Device = usbDevices.FirstOrDefault(d => d.HwModel == HardwareModel.Rak4631);
            if (rak4631Device != null)
                DiscoveredDevices.Add(rak4631Device);

            var heltecV3Device = usbDevices.FirstOrDefault(d => d.HwModel == HardwareModel.HeltecV3);
            if (heltecV3Device != null)
                DiscoveredDevices.Add(heltecV3Device);
        }
    }
}
