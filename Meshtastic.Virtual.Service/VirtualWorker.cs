using Meshtastic.Crypto;
using Meshtastic.Protobufs;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using Meshtastic.Virtual.Service.Persistance;
using System.Diagnostics;

namespace Meshtastic.Virtual.Service;

public class VirtualWorker(ILogger<VirtualWorker> logger, IVirtualStore store) : BackgroundService
{
    private UdpClient? udpClient;
    private Stopwatch stopwatch = new Stopwatch();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await store.Load();
        stopwatch.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Simulator running with MAC Address: {Macaddr}", store.Node.User.Macaddr);

            if (udpClient == null)
            {
                udpClient = new UdpClient(Resources.DEFAULT_TCP_PORT)
                {
                    EnableBroadcast = true,
                    Ttl = 64,
                };
                udpClient.JoinMulticastGroup(IPAddress.Parse("224.0.0.69"));
            }

            while (udpClient != null)
            {
                var udpData = await udpClient.ReceiveAsync(stoppingToken);
                if (udpData.Buffer.Length == 0)
                    continue;

                bool flowControl = await HandleMeshPacket(udpData);
                if (!flowControl)
                    continue;
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<bool> HandleMeshPacket(UdpReceiveResult udpData)
    {
        logger.LogInformation($"Received {udpData.Buffer.Length} bytes from {udpData.RemoteEndPoint}");
        try
        {
            var receivedPacket = MeshPacket.Parser.ParseFrom(udpData.Buffer);
            MeshPacket? responsePacket = null;
            if (receivedPacket == null)
            {
                logger.LogWarning("Unable to parse packet, ignoring");
                return false;
            }
            if (receivedPacket.Encrypted.Length == 0)
            {
                logger.LogWarning("Received unencrypted packet... ignoring");
                return false;
            }

            var data = TryDecryptPacket(receivedPacket);
            if (receivedPacket == null || data == null)
            {
                logger.LogWarning("Unable to decrypt packet, ignoring");
                return false;
            }
            logger.LogInformation($"Decrypted packet: {data}");

            // Handle node info request
            if (data.Portnum == PortNum.NodeinfoApp && data.WantResponse == true)
            {
                responsePacket = CreateMeshPacket(new Protobufs.Data
                {
                    Portnum = PortNum.NodeinfoApp,
                    Payload = store.Node.User.ToByteString(),
                    WantResponse = false,
                });
            }
            else if (data.Portnum == PortNum.TelemetryApp && data.WantResponse == true)
            {
                // Handle telemetry data
                var telemetryData = new Protobufs.Data
                {
                    Portnum = PortNum.TelemetryApp,
                    Payload = new Telemetry
                    {
                        DeviceMetrics = new DeviceMetrics
                        {
                            BatteryLevel = 100,
                            AirUtilTx = 0,
                            ChannelUtilization = 0,
                            UptimeSeconds = (uint)stopwatch.Elapsed.TotalSeconds,
                            Voltage = 0,
                        },
                    }.ToByteString(),
                    WantResponse = false,
                };
                responsePacket = CreateMeshPacket(telemetryData);
            }
            if (responsePacket == null)
                return false; // No response needed

            var responseBytes = responsePacket.ToByteArray();
            await udpClient!.SendAsync(responseBytes, responseBytes.Length, udpData.RemoteEndPoint);
            logger.LogInformation($"Sent node info response to {udpData.RemoteEndPoint}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error decrypting packet");
            return false;
        }

        return true;
    }

    private MeshPacket CreateMeshPacket(Protobufs.Data data, uint to = uint.MaxValue)
    {
        var packetId = (uint)Random.Shared.Next(int.MinValue, int.MaxValue);
        var nonce = new NonceGenerator(store.MyNodeInfo.MyNodeNum, packetId).Create();

        var encrypted = PacketEncryption.TransformPacket(
            data.ToByteArray(),
            nonce,
            Resources.DEFAULT_PSK
        );

        return new MeshPacket
        {
            Id = packetId,
            From = store.MyNodeInfo.MyNodeNum,
            To = to, // Broadcast
            HopLimit = 3,
            HopStart = 0,
            Channel = 8, //FIXME: hash function
            Priority = MeshPacket.Types.Priority.Background,
            Encrypted = ByteString.CopyFrom(encrypted)
        };
    }

    private Protobufs.Data? TryDecryptPacket(MeshPacket packet)
    {
        Protobufs.Data payload;
        var nonce = new NonceGenerator(packet.From, packet.Id).Create();

        foreach (var channel in store.Channels)
        {
            byte[] decrypted = PacketEncryption.TransformPacket(packet.Encrypted.Span.ToArray(), nonce, channel.Settings.Psk.ToByteArray());
            payload = Protobufs.Data.Parser.ParseFrom(decrypted);

            if (payload.Portnum > PortNum.UnknownApp && payload.Payload.Length > 0)
                return payload;
         }
        // Was not able to decrypt the payload
        return null;
    }
}
