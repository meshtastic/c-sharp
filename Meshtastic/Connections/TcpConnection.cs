using System.Net.Sockets;
using System.Text;
using Meshtastic.Data;

namespace Meshtastic.Connections;

public class TcpConnection : DeviceConnection, IDisposable
{
    private readonly TcpClient client;
    private NetworkStream? networkStream;
    private const int DEFAULT_BUFFER_SIZE = 64;

    public TcpConnection(string host, int port = Resources.DEFAULT_TCP_PORT)
    {
        client = new TcpClient(host, port)
        {
            ReceiveBufferSize = DEFAULT_BUFFER_SIZE,
            NoDelay = true
        };
    }

    public override async Task Monitor() 
    {
        try
        {
            var networkStream = client.GetStream();
            while (networkStream.CanRead) 
            {
                byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
                int length = await networkStream.ReadAsync(buffer);
                string data = Encoding.UTF8.GetString(buffer.AsSpan(0, length));
            }
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
            await Task.Delay(100);
            var toRadio = PacketFraming.CreatePacket(data);
            networkStream = client.GetStream();
            await networkStream.WriteAsync(toRadio);
            await ReadFromRadio(isComplete);
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task ReadFromRadio(Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        if (networkStream == null)
            throw new ApplicationException("Could not establish network stream");

        await networkStream.FlushAsync();
        var buffer = new byte[DEFAULT_BUFFER_SIZE];
        while (networkStream.CanRead)
        {
            await networkStream.ReadAsync(buffer);
            foreach (var item in buffer)
            {
                if (await ParsePackets(item, isComplete))
                    return;
            }

            await Task.Delay(10);
        }
    }

    public void Dispose() => client?.Dispose();
}