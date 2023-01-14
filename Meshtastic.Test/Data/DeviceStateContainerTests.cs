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
        deviceStateContainer.LocalConfig.Lora.TxPower.Should().Be(100);
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
        deviceStateContainer.LocalModuleConfig.Mqtt.Address.Should().Be("derp.com");
    }

    [Test]
    public void GetAdminChannelIndex_Should_DefaultToZero()
    {
        var deviceStateContainer = new DeviceStateContainer();
        var result = deviceStateContainer.GetAdminChannelIndex();
        result.Should().Be(0);
    }

    [Test]
    public void GetAdminChannelIndex_Should_ReturnIndexOfValidAdminChannel ()
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
        result.Should().Be(3);
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
        result.Should().Be(3);
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
        result.Should().Be(7);
    }

    [Test]
    public void GetNodeDisplayName_StateUnderTest_ExpectedBehavior()
    {

    }
}
