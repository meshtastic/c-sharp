using Meshtastic.Cli.Commands;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class MetadataCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        rootCommand = GetRootCommand();
        var command = new MetadataCommand("metadata", "metadata description", portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task MetadataCommand_Should_Succeed_ForValidArgs()
    {
        var result = await rootCommand.InvokeAsync("metadata --port SIMPORT", Console);
        result.Should().Be(0);
    }
}
