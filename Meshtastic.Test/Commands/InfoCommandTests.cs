using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class InfoCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new InfoCommand("info", "info description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task InfoCommand_Should_Succeed_ForValidCoords()
    {
        var result = await rootCommand.InvokeAsync("info --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
