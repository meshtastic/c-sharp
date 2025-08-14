using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Meshtastic.Discovery;

public class DeviceDiscovery(ILogger<DeviceDiscovery>? logger = null)
{
    private readonly ILogger<DeviceDiscovery>? _logger = logger;
    
    // Known Meshtastic vendor IDs
    private static readonly HashSet<string> MeshtasticVendorIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "239A",
        "10C4",
        "1A86",
        "0403",
        "2E8A",
        "2886",
    };

    public IEnumerable<MeshtasticDevice> DiscoverUsbDevices()
    {
        var devices = new List<MeshtasticDevice>();

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                devices.AddRange(DiscoverLinuxUsbDevices());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                devices.AddRange(DiscoverMacOSUsbDevices());
            }
            // Note: Windows support would require System.Management package
            // For cross-platform compatibility, we focus on Linux and macOS
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error discovering USB devices");
        }

        return devices;
    }

    private IEnumerable<MeshtasticDevice> DiscoverLinuxUsbDevices()
    {
        var devices = new List<MeshtasticDevice>();
        var usbDevicesPath = "/sys/bus/usb/devices";
        
        if (!Directory.Exists(usbDevicesPath))
            return devices;

        try
        {
            var serialPorts = SerialConnection.ListPorts();
            
            foreach (var deviceDir in Directory.GetDirectories(usbDevicesPath))
            {
                try
                {
                    var vendorFile = Path.Combine(deviceDir, "idVendor");
                    var productFile = Path.Combine(deviceDir, "idProduct");
                    
                    if (File.Exists(vendorFile) && File.Exists(productFile))
                    {
                        var vendorId = File.ReadAllText(vendorFile).Trim().ToUpper();
                        var productId = File.ReadAllText(productFile).Trim().ToUpper();
                        
                        if (MeshtasticVendorIds.Contains(vendorId))
                        {
                            var serialPort = FindLinuxSerialPort(deviceDir, serialPorts);
                            if (!string.IsNullOrEmpty(serialPort))
                            {
                                var manufacturer = TryReadFile(Path.Combine(deviceDir, "manufacturer"));
                                var product = TryReadFile(Path.Combine(deviceDir, "product"));
                                
                                devices.Add(new MeshtasticDevice
                                {
                                    Name = $"{manufacturer} {product}".Trim(),
                                    SerialPort = serialPort,
                                    VendorId = vendorId,
                                    ProductId = productId,
                                    ConnectionType = ConnectionType.Serial
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Error reading USB device info from {DeviceDir}", deviceDir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error discovering Linux USB devices");
        }

        return devices;
    }

    private IEnumerable<MeshtasticDevice> DiscoverMacOSUsbDevices()
    {
        var devices = new List<MeshtasticDevice>();
        
        try
        {
            // Use system_profiler to get USB device information
            var processInfo = new ProcessStartInfo
            {
                FileName = "system_profiler",
                Arguments = "SPUSBDataType -xml",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                devices.AddRange(ParseMacOSSystemProfiler(output));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error discovering macOS USB devices");
        }

        return devices;
    }

    private static string? FindLinuxSerialPort(string deviceDir, string[] availablePorts)
    {
        try
        {
            // Look for tty subdirectories
            var ttyDirs = Directory.GetDirectories(deviceDir, "*tty*", SearchOption.AllDirectories);
            foreach (var ttyDir in ttyDirs)
            {
                var ttyName = Path.GetFileName(ttyDir);
                var portPath = $"/dev/{ttyName}";
                if (availablePorts.Contains(portPath))
                {
                    return portPath;
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    private static string TryReadFile(string filePath)
    {
        try
        {
            return File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private List<MeshtasticDevice> ParseMacOSSystemProfiler(string xmlOutput)
    {
        var devices = new List<MeshtasticDevice>();
        var serialPorts = SerialConnection.ListPorts();
        
        try
        {
            var doc = XDocument.Parse(xmlOutput);
            var usbDevices = new List<(string name, string vendorId, string productId, string? bsdName, string? manufacturer)>();
            
            // Navigate through the plist structure to find USB devices
            var arrayElement = doc.Root?.Element("array");
            if (arrayElement != null)
            {
                foreach (var dictElement in arrayElement.Elements("dict"))
                {
                    if (FindKeyValue(dictElement, "_items") is XElement itemsArray)
                    {
                        ExtractUsbDevicesFromItems(itemsArray, usbDevices);
                    }
                }
            }
            
            // Match USB devices to available serial ports
            foreach (var (name, vendorId, productId, bsdName, manufacturer) in usbDevices)
            {
                if (IsKnownVendor(vendorId))
                {
                    string? serialPort = null;
                    
                    // First try to use the bsd_name if available
                    if (!string.IsNullOrEmpty(bsdName))
                    {
                        var bsdPath = $"/dev/{bsdName}";
                        if (serialPorts.Contains(bsdPath))
                        {
                            serialPort = bsdPath;
                        }
                    }
                    
                    // Fallback: try to match by USB serial pattern
                    serialPort ??= serialPorts.FirstOrDefault(p => 
                            p.Contains("usbmodem") || p.Contains("usbserial"));
                    
                    if (serialPort != null)
                    {
                        devices.Add(new MeshtasticDevice
                        {
                            Name = !string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(name) 
                                ? $"{manufacturer} {name}" 
                                : name ?? "Unknown USB Device",
                            SerialPort = serialPort,
                            VendorId = vendorId,
                            ProductId = productId,
                            ConnectionType = ConnectionType.Serial
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Failed to parse system_profiler XML, falling back to simple port detection");
            
            // Fallback: Look for patterns that might indicate USB-serial devices
            foreach (var port in serialPorts)
            {
                if (port.Contains("usbmodem") || port.Contains("usbserial"))
                {
                    devices.Add(new MeshtasticDevice
                    {
                        Name = $"USB Serial Device ({Path.GetFileName(port)})",
                        SerialPort = port,
                        ConnectionType = ConnectionType.Serial
                    });
                }
            }
        }
        
        return devices;
    }

    private void ExtractUsbDevicesFromItems(XElement itemsElement, List<(string name, string vendorId, string productId, string? bsdName, string? manufacturer)> devices)
    {
        foreach (var dictElement in itemsElement.Elements("dict"))
        {
            var name = FindKeyValue(dictElement, "_name") as string;
            var vendorId = FindKeyValue(dictElement, "vendor_id") as string;
            var productId = FindKeyValue(dictElement, "product_id") as string;
            var bsdName = FindKeyValue(dictElement, "bsd_name") as string;
            var manufacturer = FindKeyValue(dictElement, "manufacturer") as string;
            
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(vendorId) && !string.IsNullOrEmpty(productId))
            {
                devices.Add((name, vendorId, productId, bsdName, manufacturer));
            }
            
            // Recursively check nested _items arrays
            var nestedItemsArray = FindKeyValue(dictElement, "_items") as XElement;
            if (nestedItemsArray != null)
            {
                ExtractUsbDevicesFromItems(nestedItemsArray, devices);
            }
        }
    }

    private static object? FindKeyValue(XElement dictElement, string key)
    {
        var elements = dictElement.Elements().ToList();
        
        for (int i = 0; i < elements.Count - 1; i++)
        {
            if (elements[i].Name == "key" && elements[i].Value == key)
            {
                var valueElement = elements[i + 1];
                if (valueElement.Name == "string")
                {
                    return valueElement.Value;
                }
                else if (valueElement.Name == "array")
                {
                    return valueElement;
                }
            }
        }
        
        return null;
    }

    private static bool IsKnownVendor(string vendorId)
    {
        // Remove 0x prefix if present and check if it's in our known vendor list
        var cleanVendorId = vendorId.Replace("0x", "").Replace("0X", "");
        return MeshtasticVendorIds.Any(v => cleanVendorId.Contains(v, StringComparison.OrdinalIgnoreCase));
    }
}
