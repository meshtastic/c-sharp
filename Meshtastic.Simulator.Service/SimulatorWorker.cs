using Meshtastic.Crypto;
using Meshtastic.Protobufs;
using System.Net;
using System.Net.Sockets;
using Meshtastic.Extensions;
using Google.Protobuf;
using System.Net.NetworkInformation;

namespace Meshtastic.Simulator.Service;

public class SimulatorWorker(ILogger<SimulatorWorker> logger) : BackgroundService
{
    private UdpClient? udpClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // logger.LogInformation("Simulator running with MAC Address: {MacAddress}", macAddress);

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
            var packet = MeshPacket.Parser.ParseFrom(udpData.Buffer);
            if (packet == null)
            {
                logger.LogWarning("Unable to parse packet, ignoring");
                return false;
            }
            if (packet.Encrypted.Length == 0)
            {
                logger.LogWarning("Received unencrypted packet... ignoring");
                return false;
            }

            var data = TryDecryptPacket(packet);
            if (packet == null || data == null)
            {
                logger.LogWarning("Unable to decrypt packet, ignoring");
                return false;
            }
            logger.LogInformation($"Decrypted packet: {data}");

            // Handle node info request
            // if (data.Portnum == PortNum.NodeinfoApp && data.WantResponse == true)
            // {
                var macAddress = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress())
                    .First();
                byte[] nodeNum = [.. macAddress.GetAddressBytes().TakeLast(4)];
                var shortName = Convert.ToHexString(nodeNum);
                var user = new User
                {
                    HwModel = HardwareModel.Portduino,
                    Macaddr = ByteString.CopyFrom(macAddress.GetAddressBytes()),
                    LongName = $"Simulator_{shortName}",
                    ShortName = shortName,
                    Id = $"!{Convert.ToHexString([.. macAddress.GetAddressBytes().TakeLast(8)])}",
                    PublicKey = ByteString.CopyFrom(new byte[32]),
                };
                var packetId = (uint)Random.Shared.Next(int.MinValue, int.MaxValue);
                var from = BitConverter.ToUInt32(nodeNum, 0);
                // byte[] payload = new byte[233];
                var nodeInfo = new Protobufs.Data
                {
                    Portnum = PortNum.NodeinfoApp,
                    Payload = user.ToByteString(),
                    WantResponse = false,
                }.ToByteArray();
                // Array.Copy(nodeInfo, payload, nodeInfo.Length);

                MeshPacket responsePacket = new()
                {
                    Id = packetId,
                    From = from,
                    To = UInt32.MaxValue, // Broadcast
                    HopLimit = 3,
                    HopStart = 0,
                    Priority = MeshPacket.Types.Priority.Background,
                    Encrypted = ByteString.CopyFrom(PacketEncryption.TransformPacket(nodeInfo, new NonceGenerator(from, packetId).Create(), Resources.DEFAULT_PSK))
                };
                var responseBytes = responsePacket.ToByteArray();
                await udpClient!.SendAsync(responseBytes, responseBytes.Length, udpData.RemoteEndPoint);
                logger.LogInformation($"Sent node info response to {udpData.RemoteEndPoint}");
            // }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error decrypting packet");
        }

        return true;
    }

    private static Protobufs.Data? TryDecryptPacket(MeshPacket packet)
    {
        Protobufs.Data payload;
        var nonce = new NonceGenerator(packet.From, packet.Id).Create();
        byte[] decrypted = PacketEncryption.TransformPacket(packet.Encrypted.Span.ToArray(), nonce, Resources.DEFAULT_PSK);
        payload = Protobufs.Data.Parser.ParseFrom(decrypted);

        if (payload.Portnum > PortNum.UnknownApp && payload.Payload.Length > 0)
            return payload;

        // Was not able to decrypt the payload
        return null;
    }
}
