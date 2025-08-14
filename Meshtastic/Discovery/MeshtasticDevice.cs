using System;
using Meshtastic.Protobufs;

namespace Meshtastic.Discovery;

public class MeshtasticDevice
{
    public string? Name { get; set; }
    public string? SerialPort { get; set; }
    public HardwareModel? HwModel
    { 
        get
        {
            if (string.IsNullOrEmpty(VendorId) || string.IsNullOrEmpty(ProductId))
                return null;

            if (Name?.Contains("TRACKER L1", StringComparison.OrdinalIgnoreCase) == true)
                return HardwareModel.SeeedWioTrackerL1;

            if (Name?.Contains("RAK4631", StringComparison.OrdinalIgnoreCase) == true)
                return HardwareModel.Rak4631;

            return DetectHardwareModel(VendorId, ProductId);
        }
    }
    
    private static HardwareModel DetectHardwareModel(string vendorId, string productId)
    {
        // Clean vendor ID by removing 0x prefix
        var cleanVendorId = vendorId.Replace("0x", "").Replace("0X", "").ToUpper();

        return cleanVendorId switch
        {
            var id when id.Contains("239A") => HardwareModel.Unset, // An NRF52 device
            var id when id.Contains("10C4") => HardwareModel.HeltecV3, // Silicon Labs CP210x - Heltec V3 probably
            _ => HardwareModel.Unset
        };
    }
    public string? DeviceArchitecture { get; set; }
    public string? VendorId { get; set; }
    public string? ProductId { get; set; }
    public string? BluetoothAddress { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? Host { get; set; }

    public ConnectionType ConnectionType { get; set; }
}

public enum ConnectionType
{
    Serial,
    Bluetooth,
    Tcp,
    Virtual
}