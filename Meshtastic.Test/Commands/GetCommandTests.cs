using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class GetCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var settingsOptions = new Option<IEnumerable<string>>("--setting");
        var channelCommand = new GetCommand("get", "get description", settingsOptions, portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(channelCommand);
    }

    [Test]
    public async Task Get_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("get --setting Power.LsSecs --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
