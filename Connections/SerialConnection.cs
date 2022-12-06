using System.IO.Ports;
using System.Text;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public class SerialConnection : IConnection
{
    private readonly SerialPort serialPort;

    public SerialConnection(string port, int baudRate = Resources.DEFAULT_BAUD_RATE)
    {
        serialPort = new SerialPort(port, baudRate);
    }

    // public void OnFromRadio(Action<byte[]> action) 
    // {
    //     serialPort.DataReceived += (sender, e) => {
    //         action.Invoke(UTF8Encoding.Default.GetBytes(serialPort.ReadExisting()));
    //     };
    // }

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
            var toRadio = PacketFramer.CreatePacket(data);
            serialPort.Open();
            serialPort.Write(toRadio, 0, toRadio.Length);
            while (serialPort.IsOpen) 
            {
            if (serialPort.BytesToRead > 0) {
                Console.WriteLine(serialPort.ReadExisting());
                // var fromRadio = FromRadio.Parser.ParseFrom();
                // Console.WriteLine(fromRadio);
            }
            await Task.Delay(10);
            }
            await Task.FromResult(0);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
        Console.WriteLine("Serial disconnected");
    }
}