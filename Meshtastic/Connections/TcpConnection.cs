using System.Net;
using System.Net.Sockets;
using System.Text;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using System;

namespace Meshtastic.Connections;

public class TcpConnection : DeviceConnection, IDisposable
{
    private readonly TcpClient client;
    private NetworkStream? networkStream;

    public TcpConnection(string host, int port = Resources.DEFAULT_TCP_PORT)
    {
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
                int length = await networkStream.ReadAsync(buffer);
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
            networkStream = client.GetStream();
            await networkStream.WriteAsync(PacketFraming.SERIAL_PREAMBLE);
            await networkStream.FlushAsync();
            await networkStream.WriteAsync(data);

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
            if (ParsePackets((byte)networkStream.ReadByte(), isComplete))
                return;

            await Task.Delay(10);
        }
    }

    public void Dispose() => client?.Dispose();
}