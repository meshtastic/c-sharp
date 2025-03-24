

using System.Net.NetworkInformation;
using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Simulator.Service.Persistance;

public class SimulatorStore(IFilePersistance filePersistance) : ISimulatorStore
{
    private readonly DeviceStateContainer deviceStateContainer = new();

    public User User => deviceStateContainer.User;
    public LocalConfig LocalConfig => deviceStateContainer.LocalConfig;
    public LocalModuleConfig LocalModuleConfig => deviceStateContainer.LocalModuleConfig;
    public List<Channel> Channels => deviceStateContainer.Channels;
    public ChannelSet ChannelSettings => deviceStateContainer.ChannelSettings;
    public MyNodeInfo MyNodeInfo => deviceStateContainer.MyNodeInfo;
    public List<NodeInfo> Nodes => deviceStateContainer.Nodes;

    public async Task Load()
    {
        var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress())
                .First();

        byte[] nodeNum = [.. macAddress.GetAddressBytes().TakeLast(4)];
        var shortName = Convert.ToHexString(nodeNum)[^4..];

        if (await filePersistance.Exists(FilePaths.USER_FILE))
        {
            deviceStateContainer.User = User.Parser.ParseFrom(await filePersistance.Load(FilePaths.USER_FILE));
        }
        else
        {
            var user = new User
            {
                HwModel = HardwareModel.Portduino,
                Macaddr = ByteString.CopyFrom(macAddress.GetAddressBytes()),
                LongName = $"Simulator_{shortName}",
                ShortName = shortName,
            };
            deviceStateContainer.User = user;
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
        if (await filePersistance.Exists(FilePaths.CONFIG_FILE))
        {
            deviceStateContainer.LocalConfig = LocalConfig.Parser.ParseFrom(await filePersistance.Load(FilePaths.CONFIG_FILE));
        }
        if (await filePersistance.Exists(FilePaths.MODULE_FILE))
        {
            deviceStateContainer.LocalModuleConfig = LocalModuleConfig.Parser.ParseFrom(await filePersistance.Load(FilePaths.MODULE_FILE));
        }
        if (await filePersistance.Exists(FilePaths.CHANNELS_FILE))
        {
            // FIXME: deviceStateContainer.Channels = 
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
    }
    public async Task Save()
    {
        await filePersistance.Save(FilePaths.USER_FILE, deviceStateContainer.User.ToByteArray());
        // await filePersistance.Save(FilePaths.CHANNELS_FILE, deviceStateContainer.Channels);
        await filePersistance.Save(FilePaths.CONFIG_FILE, deviceStateContainer.LocalConfig.ToByteArray());
        await filePersistance.Save(FilePaths.MODULE_FILE, deviceStateContainer.LocalModuleConfig.ToByteArray());
    }
}