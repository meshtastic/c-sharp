using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class FileCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new FileCommand("file", "file description", portOption, hostOption, outputOption, logLevelOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task FileCommand_Should_Fail_ForEmptyOrNullText()
    {
        var result = await rootCommand.InvokeAsync("file --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Required argument missing for command: 'file'");
    }

    [Test]
    public async Task FileCommand_Should_Succeed_ForValidText()
    {
        var result = await rootCommand.InvokeAsync("file 'Butt.txt' --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
