using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class MonitorCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new MonitorCommand("monitor", "monitor description", portOption, hostOption, outputOption, logLevelOption);
        rootCommand.AddCommand(command);
    }

    //[Test]
    //public async Task MonitorCommand_Should_Succeed_ForValidCoords()
    //{
    //    var result = await rootCommand.InvokeAsync("monitor --port SIMPORT", Console);
    //    result.Should().Be(0);
    //}
}
