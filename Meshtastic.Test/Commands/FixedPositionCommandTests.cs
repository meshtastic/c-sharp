using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class FixedPositionCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new FixedPositionCommand("fixed-position", "fixed-position description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task FixedPositionCommand_Should_Fail_ForInvalidLat()
    {
        var result = await rootCommand.InvokeAsync("fixed-position -91 -90.023 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid latitude");
        result = await rootCommand.InvokeAsync("fixed-position 91 -90.023 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid latitude");
    }

    [Test]
    public async Task FixedPositionCommand_Should_Fail_ForInvalidLon()
    {
        var result = await rootCommand.InvokeAsync("fixed-position 34.00 -181 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid longitude");
        result = await rootCommand.InvokeAsync("fixed-position 34.00 -181 --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Invalid longitude");
    }
}
