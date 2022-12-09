using System.IO.Ports;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public class SerialConnection : DeviceConnection
{
    private readonly SerialPort serialPort;
    public SerialConnection(string port, int baudRate = Resources.DEFAULT_BAUD_RATE)
    {
        serialPort = new SerialPort(port, baudRate);
        serialPort.Handshake = Handshake.None;
    }

    public override async Task Monitor() 
    {
        try
        {
            serialPort.Open();
            while (serialPort.IsOpen) 
            {
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

    public override async Task WriteToRadio(byte[] data, Func<FromRadio, DeviceStateContainer, bool> isComplete)
    {
        try
        {
            var toRadio = PacketFraming.CreatePacket(data);
            serialPort.Open();
            serialPort.Write(PacketFraming.SERIAL_PREAMBLE, 0, PacketFraming.SERIAL_PREAMBLE.Length);
            serialPort.DiscardInBuffer();
            await Task.Delay(1000);
            serialPort.Write(toRadio, 0, toRadio.Length);
            await Task.Delay(100);
            await ReadFromRadio(isComplete);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task ReadFromRadio(Func<FromRadio, DeviceStateContainer, bool> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        while (serialPort.IsOpen)
        {
            if (serialPort.BytesToRead == 0)
                continue;

            var item = (byte)serialPort.ReadByte();
            if (ParsePackets(item, isComplete))
                return;
        }
        await Task.FromResult(0);
    }
}