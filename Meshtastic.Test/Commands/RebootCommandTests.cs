using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class RebootCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new RebootCommand("reboot", "reboot description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task RebootCommand_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("reboot --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
