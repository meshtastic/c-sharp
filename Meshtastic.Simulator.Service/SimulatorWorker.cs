using Meshtastic.Crypto;
using Meshtastic.Protobufs;
using System.Net;
using System.Net.Sockets;

namespace Meshtastic.Simulator.Service;

public class SimulatorWorker(ILogger<SimulatorWorker> logger) : BackgroundService
{
    private UdpClient? udpClient;
    private IPEndPoint remoteEndpoint = new(IPAddress.Any, Resources.DEFAULT_TCP_PORT);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Butt");

            if (udpClient == null)
            {
                udpClient = new UdpClient(Resources.DEFAULT_TCP_PORT)
                {
                    EnableBroadcast = true,
                    Ttl = 64,
                };
                await udpClient.Client.ConnectAsync(IPAddress.Parse("224.0.0.69"), Resources.DEFAULT_TCP_PORT);
                //udpClient.Connect(IPAddress.Parse("224.0.0.69"), Resources.DEFAULT_TCP_PORT);
            }

            if (udpClient.Client.Available > 0)
            {
                var data = udpClient.Receive(ref remoteEndpoint);
                var packet = MeshPacket.Parser.ParseFrom(data);
                var nonce = new NonceGenerator(packet.From, packet.Id).Create();
                byte[] decrypted = PacketEncryption.TransformPacket(packet.Encrypted.Span.ToArray(), nonce, Convert.FromBase64String("AQ=="));

                logger.LogInformation(packet.ToString());
                //udpClient.Send([1], 1, remoteEndpoint); // if data is received reply letting the client know that we got his data          
            }
            await Task.Delay(100, stoppingToken);
        }
    }
}
