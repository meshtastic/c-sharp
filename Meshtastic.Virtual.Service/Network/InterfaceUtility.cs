

using System.Net.NetworkInformation;

namespace Meshtastic.Virtual.Service.Network;

public static class InterfaceUtility
{
    public static PhysicalAddress GetMacAddress()
    {
        return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress())
            .First();
    }
}