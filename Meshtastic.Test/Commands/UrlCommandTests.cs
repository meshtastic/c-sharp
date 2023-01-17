using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class UrlCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new UrlCommand("url", "url description", portOption, hostOption, outputOption, logLevelOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task UrlCommand_Should_Succeed_ForValidSetArgs()
    {
        var result = await rootCommand.InvokeAsync("url get --port SIMPORT", Console);
        result.Should().Be(0);
    }

    [Test]
    public async Task UrlCommand_Should_Succeed_ForValidGetArgs()
    {
        var result = await rootCommand.InvokeAsync("url set http://meshtastic.org/#e/1235 --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
