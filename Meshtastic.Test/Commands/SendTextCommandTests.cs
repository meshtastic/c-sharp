using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class SendTextCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new SendTextCommand("text", "text description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task SendTextCommand_Should_Fail_ForEmptyOrNullText()
    {
        var result = await rootCommand.InvokeAsync("text --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Required argument missing for command: 'text'"); 
        result = await rootCommand.InvokeAsync("text '' --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Required argument missing for command: 'text'");
    }
}
