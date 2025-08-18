

using System.Net.NetworkInformation;
using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using Meshtastic.Virtual.Service.Network;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Virtual.Service.Persistance;

public class VirtualStore(IFilePersistance filePersistance) : IVirtualStore
{
    private readonly DeviceStateContainer deviceStateContainer = new();

    public LocalConfig LocalConfig => deviceStateContainer.LocalConfig;
    public LocalModuleConfig LocalModuleConfig => deviceStateContainer.LocalModuleConfig;
    public List<Channel> Channels => deviceStateContainer.Channels;
    public ChannelSet ChannelSettings => deviceStateContainer.ChannelSettings;
    public MyNodeInfo MyNodeInfo => deviceStateContainer.MyNodeInfo;
    public NodeInfo Node => deviceStateContainer.Node;
    public List<NodeInfo> Nodes => deviceStateContainer.Nodes;

    public async Task Load()
    {
        var macAddress = InterfaceUtility.GetMacAddress();
        byte[] nodeNum = [.. macAddress.GetAddressBytes().TakeLast(4)];
        var shortName = Convert.ToHexString(nodeNum)[^4..];

        if (await filePersistance.Exists(FilePaths.CONFIG_FILE))
        {
            deviceStateContainer.LocalConfig = LocalConfig.Parser.ParseFrom(await filePersistance.Load(FilePaths.CONFIG_FILE));
        }
        else
        {
            var generator = new Ed25519KeyPairGenerator();
            var parameters = new Ed25519KeyGenerationParameters(new SecureRandom());
            generator.Init(parameters);
            var keyPair = generator.GenerateKeyPair();
            var privateKey = ((Ed25519PrivateKeyParameters)keyPair.Private).GetEncoded();
            var publicKey = ((Ed25519PublicKeyParameters)keyPair.Public).GetEncoded();

            deviceStateContainer.LocalConfig = new LocalConfig
            {
                Bluetooth = new BluetoothConfig
                {
                    Enabled = false
                },
                Device = new DeviceConfig
                {
                    Role = DeviceConfig.Types.Role.Client,
                    NodeInfoBroadcastSecs = 900,
                },
                Lora = new LoRaConfig
                {
                    TxEnabled = false,
                    HopLimit = 3,
                    Region = LoRaConfig.Types.RegionCode.Unset,
                    UsePreset = true,
                    ModemPreset = LoRaConfig.Types.ModemPreset.LongFast,
                },
                Network = new NetworkConfig
                {
                    EnabledProtocols = (uint)NetworkConfig.Types.ProtocolFlags.UdpBroadcast,
                },
                Position = new PositionConfig
                {
                    GpsMode = PositionConfig.Types.GpsMode.NotPresent
                },
                Power = new PowerConfig
                {
                    IsPowerSaving = false,
                    MinWakeSecs = Int32.MaxValue,
                },
                Security = new SecurityConfig
                {
                    PrivateKey = ByteString.CopyFrom(privateKey),
                    PublicKey = ByteString.CopyFrom(publicKey),
                },
            };
        }

        if (await filePersistance.Exists(FilePaths.NODE_FILE))
        {
            deviceStateContainer.Node = NodeInfo.Parser.ParseFrom(await filePersistance.Load(FilePaths.NODE_FILE));
        }
        else
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var user = new User
            {
                HwModel = HardwareModel.Portduino,
                Macaddr = ByteString.CopyFrom(macAddress.GetAddressBytes()),
                LongName = $"Simulator_{shortName}",
                ShortName = shortName,
            };
#pragma warning restore CS0612 // Type or member is obsolete
            deviceStateContainer.Node = new NodeInfo
            {
                User = user,
                Num = BitConverter.ToUInt32(nodeNum, 0),
                Channel = 0,
                HopsAway = 0,
                IsFavorite = true,
            };
        }
        if (await filePersistance.Exists(FilePaths.MYNODEINFO_FILE))
        {
            deviceStateContainer.MyNodeInfo = MyNodeInfo.Parser.ParseFrom(await filePersistance.Load(FilePaths.MYNODEINFO_FILE));
        }
        else
        {
            deviceStateContainer.MyNodeInfo = new MyNodeInfo
            {
                MyNodeNum = BitConverter.ToUInt32(nodeNum, 0),
            };
        }
        
        if (await filePersistance.Exists(FilePaths.MODULE_FILE))
        {
            deviceStateContainer.LocalModuleConfig = LocalModuleConfig.Parser.ParseFrom(await filePersistance.Load(FilePaths.MODULE_FILE));
        }
        else {
            deviceStateContainer.LocalModuleConfig = new LocalModuleConfig
            {
                Mqtt = new MQTTConfig
                {
                    Enabled = false,
                    Address = "mqtt.meshtastic.org",
                    Username = "meshdev",
                    Password = "large4cats",
                },
            };
        }
        if (await filePersistance.Exists(FilePaths.CHANNELS_FILE))
        {
            // FIXME: Open channels
        } 
        else {
            deviceStateContainer.Channels = [
                new Channel()
                {
                    Index = 0,
                    Role = Channel.Types.Role.Primary,
                    Settings = new ChannelSettings()
                    {
                        Id = 0,
                        Psk = ByteString.CopyFrom(Resources.DEFAULT_PSK),
                        Name = "LongFast",
                    }
                },
                new Channel()
                {
                    Index = 1,
                    Role = Channel.Types.Role.Secondary,
                    Settings = new ChannelSettings()
                    {
                        Id = 1,
                        Psk = ByteString.CopyFrom(Resources.DEFAULT_PSK),
                        Name = "MediumFast",
                    }
                },
                new Channel()
                {
                    Index = 2,
                    Role = Channel.Types.Role.Secondary,
                    Settings = new ChannelSettings()
                    {
                        Id = 2,
                        Psk = ByteString.CopyFrom(Resources.DEFAULT_PSK),
                        Name = "MediumSlow",
                    }
                },
                new Channel()
                {
                    Index = 3,
                    Role = Channel.Types.Role.Secondary,
                    Settings = new ChannelSettings()
                    {
                        Id = 3,
                        Psk = ByteString.CopyFrom(Resources.DEFAULT_PSK),
                        Name = "ShortFast",
                    }
                },
                new Channel()
                {
                    Index = 4,
                    Role = Channel.Types.Role.Secondary,
                    Settings = new ChannelSettings()
                    {
                        Id = 4,
                        Psk = ByteString.CopyFrom(Resources.DEFAULT_PSK),
                        Name = "ShortTurbo",
                    }
                },
            ];
        }
        await Save();
    }

    public async Task Save()
    {
        await filePersistance.Save(FilePaths.NODE_FILE, deviceStateContainer.Node.ToByteArray());
        // await filePersistance.Save(FilePaths.CHANNELS_FILE, deviceStateContainer.Channels);
        await filePersistance.Save(FilePaths.CONFIG_FILE, deviceStateContainer.LocalConfig.ToByteArray());
        await filePersistance.Save(FilePaths.MODULE_FILE, deviceStateContainer.LocalModuleConfig.ToByteArray());
    }
}