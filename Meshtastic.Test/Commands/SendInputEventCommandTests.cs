using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class SendInputEventCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new SendInputEventCommand("input-event", "input-event description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task SendInputEventCommand_Should_Fail_ForMissingEventCode()
    {
        var result = await rootCommand.InvokeAsync("input-event --port SIMPORT", Console);
        result.ShouldBeGreaterThan(0);
        Out.Output.ShouldContain("Required argument missing for command: 'input-event'");
    }

    [Test]
    public async Task SendInputEventCommand_Should_Succeed_ForValidEventCode()
    {
        var result = await rootCommand.InvokeAsync("input-event 1 --port SIMPORT", Console);
        result.ShouldBe(0);
    }

    [Test]
    public async Task SendInputEventCommand_Should_Succeed_WithAllOptions()
    {
        var result = await rootCommand.InvokeAsync("input-event 1 --kb-char 65 --touch-x 100 --touch-y 200 --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
