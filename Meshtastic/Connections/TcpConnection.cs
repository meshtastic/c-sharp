using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Meshtastic.Connections;

public class TcpConnection : DeviceConnection, IDisposable
{
    private readonly TcpClient client;
    private NetworkStream? networkStream;
    private const int DEFAULT_BUFFER_SIZE = 32;

    public TcpConnection(ILogger logger, string host, int port = Resources.DEFAULT_TCP_PORT) : base(logger)
    {
        client = new TcpClient(host, port)
        {
            ReceiveBufferSize = DEFAULT_BUFFER_SIZE,
            NoDelay = true
        };
    }

    public override async Task<DeviceStateContainer> WriteToRadio(ToRadio packet, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete)
    {
        DeviceStateContainer.AddToRadio(packet);
        var toRadio = PacketFraming.CreatePacket(packet.ToByteArray());
        networkStream = client.GetStream();
        await networkStream.WriteAsync(toRadio);
        VerboseLogPacket(packet);
        await ReadFromRadio(isComplete);
        return DeviceStateContainer;
    }

    public override async Task WriteToRadio(ToRadio packet)
    {
        DeviceStateContainer.AddToRadio(packet);
        var toRadio = PacketFraming.CreatePacket(packet.ToByteArray());
        await networkStream!.WriteAsync(toRadio);
        await networkStream.FlushAsync();
        VerboseLogPacket(packet);
    }

    public override async Task ReadFromRadio(Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT)
    {
        if (networkStream == null)
            throw new ApplicationException("Could not establish network stream");

        var buffer = new byte[DEFAULT_BUFFER_SIZE];
        while (networkStream.CanRead)
        {
            await networkStream.ReadAsync(buffer);
            foreach (var item in buffer)
            {
                if (await ParsePackets(item, isComplete))
                    return;
            }
        }
    }

    public override void Disconnect()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        client?.Dispose();
        GC.SuppressFinalize(this);
    }
}