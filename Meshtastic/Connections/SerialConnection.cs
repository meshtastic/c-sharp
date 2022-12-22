using System.IO.Ports;
using System.Runtime.InteropServices;
using Meshtastic.Data;

namespace Meshtastic.Connections;

public class SerialConnection : DeviceConnection
{
    private readonly SerialPort serialPort;
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public SerialConnection(string port, int baudRate = Resources.DEFAULT_BAUD_RATE)
    {
        serialPort = new SerialPort(port, baudRate)
        {
            Handshake = Handshake.None,
        };
        if (!IsWindows)
            serialPort.DtrEnable = true;
    }

    public static string[] ListPorts() => SerialPort.GetPortNames();

    public override async Task Monitor() 
    {
        try
        {
            serialPort.Open();
            while (serialPort.IsOpen) 
            {
                // Hack for posix causing a RST
                if (!IsWindows)
                    serialPort.DtrEnable = false;

                if (serialPort.BytesToRead > 0) {
                    Console.Write(serialPort.ReadExisting());
                }
                await Task.Delay(10);
            }
            Console.WriteLine("Serial disconnected");
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task WriteToRadio(byte[] data, Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete)
    {
        try
        {
            await Task.Delay(500);
            var toRadio = PacketFraming.CreatePacket(data);
            if (!serialPort.IsOpen)
                serialPort.Open();
            if (!IsWindows)
                serialPort.DtrEnable = false;
            serialPort.Write(PacketFraming.SERIAL_PREAMBLE, 0, PacketFraming.SERIAL_PREAMBLE.Length);
            serialPort.DiscardInBuffer();
            serialPort.Write(toRadio, 0, toRadio.Length);
            await Task.Delay(100);
            await ReadFromRadio(isComplete);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task ReadFromRadio(Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        while (serialPort.IsOpen)
        {
            if (serialPort.BytesToRead == 0)
                continue;

            var item = (byte)serialPort.ReadByte();
            if (await ParsePackets(item, isComplete))
                return;
        }
    }
}