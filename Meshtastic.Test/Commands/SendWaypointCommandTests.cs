using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class SendWaypointCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new SendWaypointCommand("waypoint", "waypoint description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task SendWaypointCommand_Should_Fail_ForInvalidLat()
    {
        var result = await rootCommand.InvokeAsync("waypoint -91 -90.023 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid latitude");
        result = await rootCommand.InvokeAsync("waypoint 91 -90.023 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid latitude");
    }

    [Test]
    public async Task SendWaypointCommand_Should_Fail_ForInvalidLon()
    {
        var result = await rootCommand.InvokeAsync("waypoint 34.00 -181 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid longitude");
        result = await rootCommand.InvokeAsync("waypoint 34.00 -181 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid longitude");
    }

    [Test]
    public async Task SendWaypointCommand_Should_Succeed_ForValidCoords()
    {
        var result = await rootCommand.InvokeAsync("waypoint 34.00 -90 --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
