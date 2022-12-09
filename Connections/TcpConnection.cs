using System.Net;
using System.Net.Sockets;
using System.Text;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public class TcpConnection : DeviceConnection, IDisposable
{
    private readonly string host;
    private readonly int port;
    private readonly TcpClient client;
    private NetworkStream? networkStream;

    public TcpConnection(string host, int port = Resources.DEFAULT_TCP_PORT)
    {
        this.host = host;
        this.port = port;
        client = new TcpClient(host, port);
    }

    public override async Task Monitor() 
    {
        try
        {
            var networkStream = client.GetStream();
            while (networkStream.CanRead) 
            {
                byte[] buffer = new byte[1024];
                int length = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer.AsSpan(0, length));
                Console.WriteLine(data);
            }
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
            // await client.ConnectAsync(this.host, this.port);
            networkStream = client.GetStream();
            // await networkStream.WriteAsync(PacketFraming.SERIAL_PREAMBLE);
            // await networkStream.WriteAsync(data);
            // networkStream.Flush();
            // await networkStream.WriteAsync(PacketFraming.SERIAL_PREAMBLE, 0, PacketFraming.SERIAL_PREAMBLE.Length);
            // await Task.Delay(100);
            // await networkStream.WriteAsync(data, 0, data.Length);
            // await networkStream.FlushAsync();
            // Console.WriteLine("Reading");
            // byte[] buffer = new byte[32];
            // int length = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            // string text = Encoding.UTF8.GetString(buffer.AsSpan(0, length));
            // Console.WriteLine(text);
            await ReadFromRadio(isComplete);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task ReadFromRadio(Func<FromRadio, DeviceStateContainer, bool> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        if (networkStream == null)
            throw new ApplicationException("Could not establish network stream");

        while (networkStream.CanRead) 
        {
            byte[] buffer = new byte[32];
            await networkStream.ReadAsync(buffer);
            foreach (var item in buffer) 
            {
                string text = Encoding.UTF8.GetString(new [] { item }.AsSpan(0, 1));
                Console.WriteLine(text);
                if (ParsePackets(item, isComplete))
                    return;
            }
            await Task.Delay(10);
        }
        await Task.FromResult(0);
    }

    public void Dispose()
    {
        client?.Dispose();
    }
}