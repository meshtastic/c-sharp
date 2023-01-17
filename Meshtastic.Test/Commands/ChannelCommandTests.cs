using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class ChannelCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var channelCommand = new ChannelCommand("channel", "channel description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(channelCommand);
    }

    [Test]
    public async Task ChannelCommand_Should_Fail_ForIndexOutOfRange()
    {
        var result = await rootCommand.InvokeAsync("channel disable --index 10 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Channel index is out of range");
    }

    [Test]
    public async Task ChannelCommand_Should_Fail_ForEnable_DisablePrimary()
    {
        var result = await rootCommand.InvokeAsync("channel disable --index 0 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Cannot enable / disable PRIMARY channel");
        result = await rootCommand.InvokeAsync("channel enable --index 0 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Cannot enable / disable PRIMARY channel");
    }
}
