using Meshtastic.Cli.Commands;
using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;
using System.CommandLine;

namespace Meshtastic.Test.Commands;

[TestFixture]
public class SetCommandTests : CommandTestBase
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
        var command = new SetCommand("set", "set description", settingOption, portOption, hostOption, outputOption, logLevelOption, destOption, selectDestOption);
        rootCommand.AddCommand(command);
    }

    [Test]
    public async Task SetCommand_Should_Fail_ForEmptyOrNullSettings()
    {
        var result = await rootCommand.InvokeAsync("set --port SIMPORT", Console);
        result.ShouldBeGreaterThan(0);
        Out.Output.ShouldContain("Option '--setting' is required");
    }

    [Test]
    public async Task SetCommand_Should_Succeed_ForValidSetting()
    {
        var result = await rootCommand.InvokeAsync("set --setting Mqtt.Address=yourmom.com --port SIMPORT", Console);
        result.ShouldBe(0);
    }
}
