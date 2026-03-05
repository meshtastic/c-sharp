using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class ListCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new InfoCommand("list", "list description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task ListCommand_Should_Succeed_ForValidCoords()
    {
        var result = await rootCommand.InvokeAsync("list --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
