using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class ChannelCommandTests : CommandTestBase
{
    [Test]
    public async Task ChannelCommand_Should_Fail_ForIndexOutOfRange()
    {
        var rootCommand = GetRootCommand();
        var channelCommand = new ChannelCommand("channel", "channel description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(channelCommand);
        
        var result = await rootCommand.InvokeAsync("channel disable --index 10 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Channel index is out of range");
    }
}
