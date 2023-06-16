using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Meshtastic.Connections;
using TheBentern.Tak.Client;
using Meshtastic.Service.Converters;
using Meshtastic.Extensions;
using Meshtastic.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Meshtastic.Persistance.Model;
using Meshtastic.Persistance;
using Meshtastic.Data;
using Meshtastic.Service.Policies;
using Meshtastic.Service.Models;
using System.Text.RegularExpressions;

namespace Meshtastic.Service.Services;

public class DeviceConnectionService : BackgroundService
{
    private ILogger<DeviceConnectionService> Logger { get; }
    private DeviceConnection? DeviceConnection { get; set; }
    public TakClient? TakClient { get; set; }

    private IHubContext<BrokerHub, IBrokerHubClient> BrokerHubContext { get; }
    private string? TcpHostName { get; }
    public string? SerialPortName { get; }
    public string? TakPackagePath { get; }
    public ToRadioMessageFactory MessageFactory { get; }

    public DeviceConnectionService(ILogger<DeviceConnectionService> logger, IHubContext<BrokerHub, IBrokerHubClient> brokerHubContext)
    {
        Logger = logger;
        BrokerHubContext = brokerHubContext;
        TcpHostName = Environment.GetEnvironmentVariable("TCP_HOST");
        SerialPortName = Environment.GetEnvironmentVariable("SERIAL_PORT");
        TakPackagePath = Environment.GetEnvironmentVariable("TAK_PACKAGE_PATH");
        MessageFactory = new ToRadioMessageFactory();
    }

    protected override async Task ExecuteAsync(CancellationToken cancelationToken)
    {
        await RetryPolicyProvider.GetDeviceConnectionRetryPolicy(Logger, DoWork, (ex, __) =>
        {
            DeviceConnection?.Disconnect();
            TakClient?.Dispose();
            Logger.LogError($"Encountered exception, retrying: {ex}");
            return Task.CompletedTask;
        });
    }

    private async Task DoWork()
    {
        DeviceConnection = GetDeviceConnection();
        await ConnectToTakServer();

        await DeviceConnection.WriteToRadio(MessageFactory.CreateWantConfigMessage(), async (fromRadio, container) =>
        {
            await UpdateConnectionStatus(isConnected: true);
            using var context = new MeshtasticDbContext();
            if (fromRadio?.NodeInfo != null)
                await UpdateNode(fromRadio, container, context);

            if (fromRadio?.Packet == null)
                return false;

            await StoreMessage(context, fromRadio);
            await ForwardToTakServer(fromRadio, container);
            await FowardFromTakServer(container);

            return false;
        });
        await UpdateConnectionStatus(isConnected: false);
    }

    private async Task UpdateConnectionStatus(bool isConnected)
    {
        await BrokerHubContext.Clients.All.ConnectionStateChanged(new ConnectionStatusViewModel()
        {
            ConnectionType = ConnectionType.Meshtastic,
            Host = TcpHostName ?? SerialPortName!,
            IsConnected = isConnected
        });
        await BrokerHubContext.Clients.All.ConnectionStateChanged(new ConnectionStatusViewModel()
        {
            ConnectionType = ConnectionType.TAK,
            Host = TakClient?.Host ?? String.Empty,
            IsConnected = isConnected
        });
    }

    private async Task ConnectToTakServer()
    {
        if (TakPackagePath != null)
        {
            TakClient = new TakClient(TakPackagePath);
            await TakClient.ConnectAsync();
            await BrokerHubContext.Clients.All.ConnectionStateChanged(new ConnectionStatusViewModel()
            {
                ConnectionType = ConnectionType.TAK,
                Host = TakClient.Host!,
                IsConnected = true
            });
        }
        else
        {
            await BrokerHubContext.Clients.All.ConnectionStateChanged(new ConnectionStatusViewModel()
            {
                ConnectionType = ConnectionType.TAK,
                Host = String.Empty,
                IsConnected = false
            });
        }
    }

    private async Task FowardFromTakServer(DeviceStateContainer container)
    {
        if (TakClient == null) return;

        var takPackets = await TakClient.ReadEventsAsync();
        foreach (var packet in takPackets)
        {
            var cotConverter = new CotPacketToRadioConverter(packet, container);
            var toRadio = cotConverter.ConvertToRadio();
            await DeviceConnection!.WriteToRadio(toRadio);
        }
    }

    private async Task ForwardToTakServer(FromRadio fromRadio, DeviceStateContainer container)
    {
        if (TakClient == null) return;

        var meshConverter = new FromRadioCotPacketConverter(fromRadio, container);
        var cot = meshConverter.Convert();
        if (cot != null)
            await TakClient.SendAsync(cot.Raw.InnerXml.Replace("<track />", String.Empty)); // Removing empty track for now
    }

    private static async Task UpdateNode(FromRadio fromRadio, DeviceStateContainer container, MeshtasticDbContext context)
    {
        var node = await context.Nodes.FindAsync(fromRadio.NodeInfo.Num);
        if (node == null)
        {
            context.Nodes.Add(new Node
            {
                Id = fromRadio.NodeInfo.Num,
                Name = container.GetNodeDisplayName(fromRadio.NodeInfo.Num, hideNodeNum: true),
                TAKCallsign = container.GetNodeDisplayName(fromRadio.NodeInfo.Num, hideNodeNum: true),
                ShortName = fromRadio.NodeInfo?.User?.ShortName ?? String.Empty,
                HardwareModel = fromRadio.NodeInfo?.User?.HwModel.ToString() ?? String.Empty,
                LastBatteryLevel = Convert.ToInt32(fromRadio.NodeInfo?.DeviceMetrics.BatteryLevel ?? 0),
                LastAirUtilTx = fromRadio.NodeInfo?.DeviceMetrics?.AirUtilTx,
                LastChannelUtilTx = fromRadio.NodeInfo?.DeviceMetrics?.ChannelUtilization,
                LastLatitude = fromRadio.NodeInfo?.Position?.LatitudeI * 1e-7,
                LastLongitude = fromRadio.NodeInfo?.Position?.LongitudeI * 1e-7,
                LastAltitude = fromRadio.NodeInfo?.Position?.Altitude,
                LastHeardFrom = DateTime.Now
            });
        }
        else
        {
            node.Name = container.GetNodeDisplayName(fromRadio.NodeInfo.Num, hideNodeNum: true);
            node.ShortName = fromRadio.NodeInfo?.User?.ShortName ?? String.Empty;
            node.HardwareModel = fromRadio.NodeInfo?.User?.HwModel.ToString() ?? String.Empty;
            node.LastBatteryLevel = Convert.ToInt32(fromRadio.NodeInfo?.DeviceMetrics?.BatteryLevel ?? 0);
            node.LastAirUtilTx = fromRadio.NodeInfo?.DeviceMetrics?.AirUtilTx;
            node.LastChannelUtilTx = fromRadio.NodeInfo?.DeviceMetrics?.ChannelUtilization;
            node.LastLatitude = fromRadio.NodeInfo?.Position?.LatitudeI * 1e-7;
            node.LastLongitude = fromRadio.NodeInfo?.Position?.LongitudeI * 1e-7;
            node.LastAltitude = fromRadio.NodeInfo?.Position?.Altitude;
        }
        context.NodeInfos.Add(new NodeInfoPacket { NodeInfoId = fromRadio.Id, NodeId = fromRadio.NodeInfo!.Num, Payload = fromRadio.NodeInfo!.ToString() });
        await context.SaveChangesAsync();
    }

    private async Task StoreMessage(MeshtasticDbContext context, FromRadio fromRadio)
    {
        if (fromRadio.GetPayload<string>() != null)
        {
            var packet = new TextPacket() { TextId = fromRadio.Id, NodeId = fromRadio.Packet.From, Payload = fromRadio.GetPayload<string>()! };
            context.Texts.Add(packet);
            await UpdateHub(context, fromRadio, packet);
        }
        else if (fromRadio.GetPayload<Position>() != null)
        {
            var payload = fromRadio.GetPayload<Position>()!;
            var packet = new PositionPacket() { PositionId = fromRadio.Id, NodeId = fromRadio.Packet.From, Payload = payload.ToString() };
            context.Positions.Add(packet);
            await UpdatePosition(context, fromRadio, payload);
            await UpdateHub(context, fromRadio, packet);
        }
        else if (fromRadio.GetPayload<Telemetry>() != null)
        {
            var payload = fromRadio.GetPayload<Telemetry>()!;
            var packet = new TelemetryPacket() { TelemetryId = fromRadio.Id, NodeId = fromRadio.Packet.From, Payload = payload.ToString() };
            context.Telemetries.Add(packet);
            if (payload.VariantCase == Telemetry.VariantOneofCase.DeviceMetrics)
                await UpdateDeviceMetrics(context, fromRadio, payload);

            await UpdateHub(context, fromRadio, packet);
        }
        else if (fromRadio.GetPayload<Waypoint>() != null)
        {
            var packet = new WaypointPacket() { WaypointId = fromRadio.Id, NodeId = fromRadio.Packet.From, Payload = fromRadio.GetPayload<Waypoint>()!.ToString() };
            context.Waypoints.Add(packet);
            await UpdateHub(context, fromRadio, packet);
        }
        else return; // Skip packets we don't care about

        await context.SaveChangesAsync();
    }

    private async Task UpdatePosition(MeshtasticDbContext context, FromRadio fromRadio, Position payload)
    {
        var node = await context.Nodes.FindAsync(fromRadio.Packet.From);
        if (node != null)
        {
            node.LastLatitude = payload.LatitudeI * 1e-7;
            node.LastLongitude = payload.LongitudeI * 1e-7;
            node.LastAltitude = payload.Altitude;
            node.LastHeardFrom = DateTime.Now;
        }
    }

    private async Task UpdateDeviceMetrics(MeshtasticDbContext context, FromRadio fromRadio, Telemetry payload)
    {
        var node = await context.Nodes.FindAsync(fromRadio.Packet.From);
        if (node != null)
        {
            node.LastBatteryLevel = Convert.ToInt32(payload.DeviceMetrics.BatteryLevel);
            node.LastAirUtilTx = Convert.ToInt32(payload.DeviceMetrics.AirUtilTx);
            node.LastChannelUtilTx = Convert.ToInt32(payload.DeviceMetrics.ChannelUtilization);
            node.LastHeardFrom = DateTime.Now;
        }
    }
    const string regexPattern = "\"([^\"]+)\":"; // the "propertyName": pattern

    private async Task UpdateHub(MeshtasticDbContext context, FromRadio fromRadio, MeshtasticPacket packet)
    {
        var node = await context.Nodes.FindAsync(fromRadio.Packet.From) ?? new Node() { Id = fromRadio.Packet.From, Name = "Unknown" };
        
        await BrokerHubContext.Clients.All.FromRadioReceived(new FromRadioViewModel(node, fromRadio.Packet.Decoded.Portnum, Regex.Replace(packet.Payload, regexPattern, "$1:"), DateTime.Now));
    }

    private DeviceConnection GetDeviceConnection() => TcpHostName != null ? new TcpConnection(Logger, TcpHostName) : new SerialConnection(Logger, SerialPortName!);
}