using Google.Protobuf;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Data.MessageFactories;

public class AdminMessageFactory
{
    private readonly DeviceStateContainer container;
    private readonly uint? dest;

    public AdminMessageFactory(DeviceStateContainer container, uint? dest = null)
    {
        this.container = container;
        this.dest = dest;
    }

    private MeshPacket GetNewMeshPacket(AdminMessage message)
    {
        return new MeshPacket()
        {
            Channel = container.GetAdminChannelIndex(),
            WantAck = false,
            To = dest ?? container.MyNodeInfo.MyNodeNum,
            Id = (uint)(Random.Shared.NextInt64(1, 1_000_000_000)),
            HopLimit = container?.GetHopLimitOrDefault() ?? 3,
            Decoded = new Protobufs.Data()
            {
                Portnum = PortNum.AdminApp,
                Payload = message.ToByteString(),
                WantResponse = true,
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

    public MeshPacket CreateRebootMessage(int seconds, bool isOta)
    {
        if (isOta)
        {
            return GetNewMeshPacket(new AdminMessage()
            {
                RebootOtaSeconds = seconds
            });
        }
        return GetNewMeshPacket(new AdminMessage()
        {
            RebootSeconds = seconds
        });
    }

    public MeshPacket CreateEnterDfuMessage()
    {
        return GetNewMeshPacket(new AdminMessage()
        {
            EnterDfuModeRequest = true,
        });
    }

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
        return GetNewMeshPacket(new AdminMessage() { SetConfig = config! });
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
            RemoteHardwareConfig => new ModuleConfig() { RemoteHardware = instance as RemoteHardwareConfig },
            NeighborInfoConfig => new ModuleConfig() { NeighborInfo = instance as NeighborInfoConfig },
            AmbientLightingConfig => new ModuleConfig() { AmbientLighting = instance as AmbientLightingConfig },
            DetectionSensorConfig => new ModuleConfig() { DetectionSensor = instance as DetectionSensorConfig },

            _ => throw new ArgumentException("Could not determine ModuleConfig type", nameof(instance)),
        };
        return GetNewMeshPacket(new AdminMessage() { SetModuleConfig = moduleConfig! });
    }

    public MeshPacket CreateSetChannelMessage(Channel channel)
    {
        return GetNewMeshPacket(new AdminMessage() { SetChannel = channel });
    }

    public MeshPacket CreateAddContactMessage(SharedContact contact)
    {
        return GetNewMeshPacket(new AdminMessage() { AddContact = contact });
    }

    public MeshPacket CreateGetMetadataMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { GetDeviceMetadataRequest = true });
    }

    public MeshPacket CreateFactoryResetMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { FactoryResetConfig = 1 });
    }
    public MeshPacket CreateNodeDbResetMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { NodedbReset = 1 });
    }
    public MeshPacket CreateRemoveByNodenumMessage(uint nodeNum)
    {
        return GetNewMeshPacket(new AdminMessage() { RemoveByNodenum = nodeNum });
    }
    public MeshPacket CreateSetCannedMessage(string message)
    {
        return GetNewMeshPacket(new AdminMessage() { SetCannedMessageModuleMessages = message });
    }
    public MeshPacket CreateGetCannedMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { GetCannedMessageModuleMessagesRequest = true });
    }
    public MeshPacket CreateSetOwnerMessage(User user)
    {
        return GetNewMeshPacket(new AdminMessage() { SetOwner = user });
    }

    public MeshPacket CreateFixedPositionMessage(Position position)
    {
        return GetNewMeshPacket(new AdminMessage() { SetFixedPosition = position });
    }

    public MeshPacket RemovedFixedPositionMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { RemoveFixedPosition = true });
    }

    public MeshPacket CreateSendInputEventMessage(AdminMessage.Types.InputEvent inputEvent)
    {
        return GetNewMeshPacket(new AdminMessage() { SendInputEvent = inputEvent });
    }

    public MeshPacket CreateGetOwnerMessage()
    {
        return GetNewMeshPacket(new AdminMessage() { GetOwnerRequest = true });
    }
}