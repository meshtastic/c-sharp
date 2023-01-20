using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.Config.Types;

namespace Meshtastic.Test.Extensions
{
    [TestFixture]
    public class ReflectionExtensionsTests
    {
        [Test]
        public void GetSettingsOptions_Should_ThrowException_GivenNullArg()
        {
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
            var action = () => default(LocalConfig).GetSettingsOptions();
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetSettingsOptions_Should_EnumerateConfigSettings()
        {
            var list = new LocalConfig().GetSettingsOptions();
            list.Should().Contain("Device.Role");
        }

        [Test]
        public void GetSettingsOptions_Should_EnumerateModuleConfigSettings()
        {
            var list = new LocalModuleConfig().GetSettingsOptions();
            list.Should().Contain("Mqtt.Address");
        }

        [Test]
        public void GetProperties_Should_ReturnPropertyOnConfigSection()
        {
            var list = new LoRaConfig().GetProperties();
            list.Should().Contain(p => p.Name == "TxPower");
        }

        [Test]
        public void GetSettingValue_Should_ReturnPropertyValueOnConfigSection()
        {
            var lora = new LoRaConfig() { TxPower = 100 };
            var property = lora.GetProperties().First(p => p.Name == "TxPower");
            property.GetSettingValue(lora).Should().BeEquivalentTo(lora.TxPower.ToString());
        }
    }
}
