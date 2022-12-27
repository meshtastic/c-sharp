using System.IO.Ports;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using System;

namespace Meshtastic.Connections;

public class SerialConnection : DeviceConnection
{
    private readonly SerialPort serialPort;
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public SerialConnection(ILogger logger, string port, int baudRate = Resources.DEFAULT_BAUD_RATE) : base(logger)
    {
        serialPort = new SerialPort(port, baudRate)
        {
            DtrEnable = true,
            WriteBufferSize = 8,
        };
    }

    public static string[] ListPorts() => SerialPort.GetPortNames();

    public override async Task Monitor() 
    {
        Logger.LogDebug("Opening serial port...");
        serialPort.Open();
        while (serialPort.IsOpen) 
        {
            Logger.LogDebug("Opened serial port");
            if (serialPort.BytesToRead > 0) {
                Console.Write(serialPort.ReadExisting());
            }
            await Task.Delay(10);
        }
        Logger.LogDebug("Disconnected from serial");
    }

    public override async Task WriteToRadio(ToRadio data, Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete)
    {
        await Task.Delay(700);
        var toRadio = PacketFraming.CreatePacket(data.ToByteArray());
        if (!serialPort.IsOpen)
            serialPort.Open();
        await serialPort.BaseStream.WriteAsync(PacketFraming.SERIAL_PREAMBLE, 0, PacketFraming.SERIAL_PREAMBLE.Length);
        await serialPort.BaseStream.WriteAsync(toRadio, 0, toRadio.Length);
        Logger.LogDebug($"Sent: {data}");
        await ReadFromRadio(isComplete);
    }

    public override async Task ReadFromRadio(Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        while (serialPort.IsOpen)
        {
            if (serialPort.BytesToRead == 0) 
            {
                await Task.Delay(10);
                continue;
            }
            var buffer = new byte[1];
            await serialPort.BaseStream.ReadAsync(buffer);
            if (await ParsePackets(buffer.First(), isComplete))
                return;
        }
    }
}