using Meshtastic.Cli.Commands;
using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class GetCommandTests : CommandTestBase
{
    private RootCommand rootCommand;

    [SetUp]
    public void Setup()
    {
        var settingOption = new Option<IEnumerable<string>>("--setting", description: "Get or set a value on config / module-config")
        {
            AllowMultipleArgumentsPerToken = true,
            IsRequired = true,
        };
        settingOption.AddAlias("-s");
        settingOption.AddCompletions(ctx => new LocalConfig().GetSettingsOptions().Concat(new LocalModuleConfig().GetSettingsOptions()));
        rootCommand = GetRootCommand();
        var command = new GetCommand("get", "get description", settingOption, portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task SetCommand_Should_Fail_ForEmptyOrNullSettings()
    {
        var result = await rootCommand.InvokeAsync("get --port SIMPORT", Console);
        result.ShouldBeGreaterThan(0);
        Out.Output.ShouldContain("Option '--setting' is required");
    }

    [Test]
    public async Task GetCommand_Should_Succeed_ForValidSetting()
    {
        var result = await rootCommand.InvokeAsync("get --setting Mqtt.Address --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
