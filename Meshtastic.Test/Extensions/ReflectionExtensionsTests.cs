using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;

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
            var list =  new LocalConfig().GetSettingsOptions();
            list.Should().Contain("Device.Role");
        }

        [Test]
        public void GetSettingsOptions_Should_EnumerateModuleConfigSettings()
        {
            var list = new LocalModuleConfig().GetSettingsOptions();
            list.Should().Contain("Mqtt.Address");
        }
    }
}
