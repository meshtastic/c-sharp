using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class FactoryResetCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var channelCommand = new ChannelCommand("factory-reset", "factory-reset description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(channelCommand);
    }

    [Test]
    public async Task FactoryReset_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("factory-reset --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
    }
}
