// using System;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using InTheHand.BluetoothLE;
// using System.Collections.Generic;

// namespace Meshtastic.Connections;

// // Placeholder for BluetoothConnection using Plugin.BLE or similar library
// public class BluetoothConnection : DeviceConnection
// {
//     private IDevice? _device;
//     private IGattCharacteristic? _writeCharacteristic;
//     private IGattCharacteristic? _readCharacteristic;
//     private BluetoothLEScan? _scan;

//     public BluetoothConnection(ILogger logger) : base(logger) { }

//     // Example: Discover and connect to a device by name (adjust as needed)
//     public async Task<bool> ConnectAsync(string deviceName)
//     {
//         var devices = new List<IDevice>();
//         _scan = Bluetooth.ScanForDevices(async dev =>
//         {
//             if (dev.Name == deviceName)
//             {
//                 devices.Add(dev);
//                 await _scan.StopAsync();
//             }
//         });
//         await _scan.Task;
//         if (devices.Count == 0) return false;
//         _device = devices[0];
//         await _device.Gatt.ConnectAsync();
//         // TODO: Discover and assign _writeCharacteristic and _readCharacteristic
//         return true;
//     }

//     public async Task<bool> DiscoverCharacteristicsAsync(Guid serviceUuid, Guid writeCharUuid, Guid readCharUuid)
//     {
//         if (_device == null) throw new InvalidOperationException("Device not connected.");
//         var services = await _device.Gatt.GetPrimaryServicesAsync();
//         var service = services?.Find(s => s.Uuid == serviceUuid);
//         if (service == null) return false;
//         var characteristics = await service.GetCharacteristicsAsync();
//         _writeCharacteristic = characteristics?.Find(c => c.Uuid == writeCharUuid);
//         _readCharacteristic = characteristics?.Find(c => c.Uuid == readCharUuid);
//         return _writeCharacteristic != null && _readCharacteristic != null;
//     }

//     public override async Task<DeviceStateContainer> WriteToRadio(ToRadio toRadio, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete)
//     {
//         if (_writeCharacteristic == null) throw new InvalidOperationException("Not connected to a Bluetooth device.");
//         var data = toRadio.ToByteArray();
//         await _writeCharacteristic.WriteValueWithResponseAsync(data);
//         // Optionally, wait for a response using ReadFromRadio
//         return DeviceStateContainer;
//     }

//     public override async Task WriteToRadio(ToRadio toRadio)
//     {
//         if (_writeCharacteristic == null) throw new InvalidOperationException("Not connected to a Bluetooth device.");
//         var data = toRadio.ToByteArray();
//         await _writeCharacteristic.WriteValueWithResponseAsync(data);
//     }

//     public override void Disconnect()
//     {
//         _device?.Gatt.Disconnect();
//         _device = null;
//         _writeCharacteristic = null;
//         _readCharacteristic = null;
//     }

//     public override async Task ReadFromRadio(Func<FromRadio?, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
//     {
//         if (_readCharacteristic == null) throw new InvalidOperationException("Not connected to a Bluetooth device.");
//         var value = await _readCharacteristic.ReadValueAsync();
//         if (value != null && value.Length > 0)
//         {
//             // Example: parse value into FromRadio
//             var fromRadioMsg = new FromDeviceMessage(Logger);
//             var fromRadio = fromRadioMsg.ParsedFromRadio(value);
//             await isComplete(fromRadio, DeviceStateContainer);
//         }
//     }
// }
