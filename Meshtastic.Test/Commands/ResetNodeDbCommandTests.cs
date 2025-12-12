using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class ResetNodeDbCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new ResetNodeDbCommand("reset-nodedb", "Reset Node Db description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task ResetNodeDbCommand_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("reset-nodedb --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
