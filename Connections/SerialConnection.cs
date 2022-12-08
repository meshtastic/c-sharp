using System.IO.Ports;
using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public class SerialConnection : IDeviceConnection
{
    public DeviceStateContainer DeviceStateContainer { get; set; } = new DeviceStateContainer();

    private readonly SerialPort serialPort;

    public SerialConnection(string port, int baudRate = Resources.DEFAULT_BAUD_RATE)
    {
        serialPort = new SerialPort(port, baudRate);
        serialPort.Handshake = Handshake.None;
    }

    public async Task Monitor() 
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

    public async Task WriteToRadio(byte[] data)
    {
        try
        {
            var toRadio = PacketFraming.CreatePacket(data);
            serialPort.Open();
            serialPort.Write(Resources.SERIAL_PREAMBLE, 0, Resources.SERIAL_PREAMBLE.Length);
            serialPort.DiscardInBuffer();
            await Task.Delay(1000);
            serialPort.Write(toRadio, 0, toRadio.Length);
            await Task.Delay(100);
            await ReadFromRadio(p => p.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.ConfigCompleteId);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task ReadFromRadio(Func<FromRadio, bool> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        List<byte> buffer = new();
        var packetLength = 0;
        while (serialPort.IsOpen)
        {
            if (serialPort.BytesToRead == 0)
                continue;

            var item = (byte)serialPort.ReadByte();

            int bufferIndex = buffer.Count();
            buffer.Add(item);
            if (bufferIndex == 0 && item != Resources.PACKET_FRAME_START[0]) 
                buffer.Clear();
            else if (bufferIndex == 1 && item != Resources.PACKET_FRAME_START[1])
                buffer.Clear();
            else if (bufferIndex >= Resources.PACKET_HEADER_LENGTH - 1) 
            {
                packetLength = (buffer[2] << 8) + buffer[3];
                // Packet fails size validation
                if (bufferIndex == Resources.PACKET_HEADER_LENGTH - 1 && packetLength > Resources.MAX_TO_FROM_RADIO_LENGTH) 
                    buffer.Clear();

                if (buffer.Count() > 0 && (bufferIndex + 1) >= (packetLength + Resources.PACKET_HEADER_LENGTH))
                {
                    try 
                    {
                        var payload = buffer.Skip(Resources.PACKET_HEADER_LENGTH).ToArray();
                        var fromRadio = FromRadio.Parser.ParseFrom(payload);
                    
                        DeviceStateContainer.AddFromRadio(fromRadio);

                        if (isComplete(fromRadio))
                        {
                            ProtobufPrinter.Print(DeviceStateContainer);
                            return;
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                    buffer.Clear();
                }
            }
        }
        await Task.FromResult(0);
    }
}