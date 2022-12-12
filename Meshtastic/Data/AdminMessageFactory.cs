using Google.Protobuf;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Data;

public class AdminMessageFactory 
{
    private readonly DeviceStateContainer container;

    public AdminMessageFactory(DeviceStateContainer container)
    {
        this.container = container;
    }

    private MeshPacket GetNewMeshPacket(AdminMessage message, uint? to = null, uint? from = null, uint? dest = null)
    {
        return new MeshPacket()
        {
            Channel = container.GetAdminChannelIndex(),
            WantAck = false,
            To = to ?? container.MyNodeInfo.MyNodeNum,
            From = from ?? container.MyNodeInfo.MyNodeNum,
            Id = (uint)Random.Shared.Next(),
            Decoded = new Protobufs.Data()
            {
                Dest = dest ?? container.MyNodeInfo.MyNodeNum,
                Portnum = PortNum.AdminApp,
                Payload = message.ToByteString(),
                WantResponse = true
            },
        };
    }

    public MeshPacket CreateBeginEditSettingsMessage() =>
        GetNewMeshPacket(new AdminMessage()
        {
            BeginEditSettings = true
        });

    public MeshPacket CreateCommitEditSettingsMessage() =>
        GetNewMeshPacket(new AdminMessage()
        {
            CommitEditSettings = true
        });

    public MeshPacket CreateSetConfigMessage(object instance)
    {
        var config = instance switch
        {
            BluetoothConfig => new Config() { Bluetooth = instance as BluetoothConfig },
            DeviceConfig => new Config() { Device = instance as DeviceConfig },
            DisplayConfig => new Config() { Display = instance as DisplayConfig },
            LoRaConfig => new Config() { Lora = instance as LoRaConfig },
            NetworkConfig => new Config() { Network = instance as NetworkConfig },
            PositionConfig => new Config() { Position = instance as PositionConfig },
            PowerConfig => new Config() { Power = instance as PowerConfig },
            _ => throw new ArgumentException("Could not determine Config type", nameof(instance)),
        };
        return GetNewMeshPacket(new AdminMessage() {  SetConfig = config! });
    }

    public MeshPacket CreateSetModuleConfigMessage(object instance)
    {
        var moduleConfig = instance switch
        {
            AudioConfig => new ModuleConfig() { Audio = instance as AudioConfig },
            CannedMessageConfig => new ModuleConfig() { CannedMessage = instance as CannedMessageConfig },
            ExternalNotificationConfig => new ModuleConfig() { ExternalNotification = instance as ExternalNotificationConfig },
            MQTTConfig => new ModuleConfig() { Mqtt = instance as MQTTConfig },
            RangeTestConfig => new ModuleConfig() { RangeTest = instance as RangeTestConfig },
            SerialConfig => new ModuleConfig() { Serial = instance as SerialConfig },
            StoreForwardConfig => new ModuleConfig() { StoreForward = instance as StoreForwardConfig },
            TelemetryConfig => new ModuleConfig() { Telemetry = instance as TelemetryConfig },
            _ => throw new ArgumentException("Could not determine ModuleConfig type", nameof(instance)),
        };
        return GetNewMeshPacket(new AdminMessage() { SetModuleConfig = moduleConfig! });
    }      
}