using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class MonitorCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

        rootCommand.AddCommand(command);
    }

    //[Test]
    //public async Task MonitorCommand_Should_Succeed_ForValidCoords()
    //{
    //    var result = await rootCommand.InvokeAsync("monitor --port SIMPORT", Console);
    //    result.Should().Be(0);
    //}
}
