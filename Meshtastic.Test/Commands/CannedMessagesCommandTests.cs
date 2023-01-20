using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class CannedMessagesCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new CannedMessagesCommand("canned-messages", "canned-messages description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task CannedMessagesCommand_Should_Fail_ForEmptyOrNullMessagesOnSet()
    {
        var result = await rootCommand.InvokeAsync("canned-messages set --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Must specify pipe delimited messages");
    }

    [Test]
    public async Task CannedMessagesCommand_Should_Fail_ForPipelessMessagesOnSet()
    {
        var result = await rootCommand.InvokeAsync("canned-messages set hello --port SIMPORT", Console);
        result.Should().BeGreaterThan(0);
        Out.Output.Should().Contain("Must specify pipe delimited messages");
    }

    [Test]
    public async Task CannedMessagesCommand_Should_Succeed_ForValidGet()
    {
        var result = await rootCommand.InvokeAsync("canned-messages get --port SIMPORT", Console);
        result.Should().Be(0);
    }

    [Test]
    public async Task CannedMessagesCommand_Should_Succeed_ForValidSet()
    {
        var result = await rootCommand.InvokeAsync("canned-messages set \"I need an alpinist|Halp\" --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
