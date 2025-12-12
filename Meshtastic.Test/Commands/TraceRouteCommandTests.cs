using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class TraceRouteCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new TraceRouteCommand("traceroute", "Traceroute description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task TraceRouteCommand_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("traceroute --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
