using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;
using static Meshtastic.Protobufs.ModuleConfig.Types;

namespace Meshtastic.Test.Data;

[TestFixture]
public class DeviceStateContainerTests
{
    [Test]
    public void AddFromRadio_Should_SetConfigSection_GivenConfig()
    {
        var deviceStateContainer = new DeviceStateContainer
        {
            LocalConfig = new LocalConfig()
        };
        var fromRadio = new FromRadio()
        {
            Config = new Config()
            {
                Lora = new LoRaConfig() { TxPower = 100 }
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        deviceStateContainer.LocalConfig.Lora.TxPower.ShouldBe(100);
    }

    [Test]
    public void AddFromRadio_Should_SetModuleConfigSection_GivenModuleConfig()
    {
        var deviceStateContainer = new DeviceStateContainer
        {
            LocalConfig = new LocalConfig()
        };
        var fromRadio = new FromRadio()
        {
            ModuleConfig = new ModuleConfig()
            {
                Mqtt = new MQTTConfig() { Address = "derp.com" }
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        deviceStateContainer.LocalModuleConfig.Mqtt.Address.ShouldBe("derp.com");
    }

    [Test]
    public void AddFromRadio_Should_AddChannel_GivenNewChannel()
    {
        var deviceStateContainer = new DeviceStateContainer
        {
            Channels = new List<Channel>()
        };
        var fromRadio = new FromRadio()
        {
            Channel = new Channel()
            {
                Index = 3,
                Role = Channel.Types.Role.Secondary,
                Settings = new ChannelSettings()
                {
                    Name = "admin"
                }
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        deviceStateContainer.Channels.ShouldContain(c => c.Settings.Name == "admin");
    }

    [Test]
    public void AddFromRadio_Should_SetMyInfo_GivenMyNodeInfo()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var fromRadio = new FromRadio()
        {
            MyInfo = new MyNodeInfo()
            {
                RebootCount = 1234
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        deviceStateContainer.MyNodeInfo.RebootCount.ShouldBe((uint)1234);
    }

    [Test]
    public void AddFromRadio_Should_AddNode_GivenNodeInfo()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var fromRadio = new FromRadio()
        {
            NodeInfo = new NodeInfo()
            {
                Num = 1234
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        deviceStateContainer.Nodes.ShouldContain(n => n.Num == 1234);
    }

    [Test]
    public void GetAdminChannelIndex_Should_DefaultToZero()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var result = deviceStateContainer.GetAdminChannelIndex();
        result.ShouldBe((uint)0);
    }

    [Test]
    public void GetAdminChannelIndex_Should_ReturnIndexOfValidAdminChannel()
    {
        var deviceStateContainer = new DeviceStateContainer();
        deviceStateContainer.Channels.Add(new Channel()
        {
            Index = 3,
            Role = Channel.Types.Role.Secondary,
            Settings = new ChannelSettings()
            {
                Name = "admin"
            }
        });
        var result = deviceStateContainer.GetAdminChannelIndex();
        result.ShouldBe((uint)3);
    }

    [Test]
    public void GetHopLimitOrDefault_Should_DefaultToThree()
    {
        var deviceStateContainer = new DeviceStateContainer
        {
            LocalConfig = new LocalConfig()
            {
                Lora = new LoRaConfig() { HopLimit = 0 }
            }
        };
        var result = deviceStateContainer.GetHopLimitOrDefault();
        result.ShouldBe((uint)3);
    }

    [Test]
    public void GetHopLimitOrDefault_Should_ReturnHopLimitFromLoraConfig()
    {
        var deviceStateContainer = new DeviceStateContainer();
        deviceStateContainer.LocalConfig = new LocalConfig()
        {
            Lora = new LoRaConfig() { HopLimit = 7 }
        };
        var result = deviceStateContainer.GetHopLimitOrDefault();
        result.ShouldBe((uint)7);
    }

    [Test]
    public void GetNodeDisplayName_Should_ReturnNumShortNameLongNameFromNodeList()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var fromRadio = new FromRadio()
        {
            NodeInfo = new NodeInfo()
            {
                Num = 1234,
                User = new User()
                {
                    LongName = "Bunghole",
                    ShortName = "BUTT"
                }
            }
        };
        deviceStateContainer.AddFromRadio(fromRadio);
        var result = deviceStateContainer.GetNodeDisplayName(1234);
        result.ShouldContain("BUTT");
        result.ShouldContain("Bunghole");
        result.ShouldContain("1234");
    }

    [Test]
    public void GetNodeDisplayName_Should_ReturnNodeNum_WhenNodeIsntPresentInList()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var result = deviceStateContainer.GetNodeDisplayName(1234);
        result.ShouldBe("1234");
    }
}
