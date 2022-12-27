using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Parsers;

public class ReflectionExtensionsTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidModuleConfigArgs_Get()
    {
        var configs = new LocalConfig().GetSettingsOptions();

        configs.Should().Contain("Display.ScreenOnSecs");
    }
}
