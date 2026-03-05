using Meshtastic.Cli.Parsers;
using static Meshtastic.Protobufs.ModuleConfig.Types.SerialConfig.Types;

namespace Meshtastic.Test.Parsers;

public class SettingParserTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidModuleConfigArgs_Get()
    {
        var parser = new SettingParser(new[] { "mqtt.enabled" });
        var result = parser.ParseSettings(isGetOnly: true);

        result.ParsedSettings.All(p => p.Setting.Name == "Enabled").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Mqtt").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value  is null).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidModuleConfigArgs_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.enabled=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.All(p => p.Setting.Name == "Enabled").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Mqtt").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value is true).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }

    //[Test]
    //public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenIntToImplicitBoolean_Set()
    //{
    //    var parser = new SettingParser(new[] { "mqtt.enabled=1" });
    //    var result = parser.ParseSettings(isGetOnly: false);

    //    result.ParsedSettings.All(p => p.Setting.Name == "Enabled").ShouldBeTrue();
    //    result.ParsedSettings.All(p => p.Section.Name == "Mqtt").ShouldBeTrue();
    //    result.ParsedSettings.All(p => p.Value is true).ShouldBeTrue();

    //    result.ValidationIssues.ShouldBeEmpty();
    //}

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenEnumValue_Set()
    {
        var parser = new SettingParser(new[] { "serial.baud=Baud19200" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.All(p => p.Setting.Name == "Baud").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Serial").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value is Serial_Baud.Baud19200).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidConfigArgs_Get()
    {
        var parser = new SettingParser(new[] { "display.screenOnSecs" });
        var result = parser.ParseSettings(isGetOnly: true);

        result.ParsedSettings.All(p => p.Setting.Name == "ScreenOnSecs").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Display").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value is null).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenBardArgs_Set()
    {
        var parser = new SettingParser(new[] { "screenOnSecs" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenBadArgs_Set()
    {
        var parser = new SettingParser(new[] { "screenOnSecs=42" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenMissingSection_Set()
    {
        var parser = new SettingParser(new[] { "doesntexist.enabled" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenMissingSection_Set()
    {
        var parser = new SettingParser(new[] { "doesntexist.enabled=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }


    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenMissingSetting_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.derp" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenMissingSetting_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.derp=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.ShouldBeEmpty();
        result.ValidationIssues.Count().ShouldBe(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenSnakeCase()
    {
        var parser = new SettingParser(new[] { "display.screen_on_secs" });
        var result = parser.ParseSettings(isGetOnly: true);

        result.ParsedSettings.All(p => p.Setting.Name == "ScreenOnSecs").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Display").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value is null).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }

    [Test]
    public void ParseSettings_Should_SetFloatValue()
    {
        var parser = new SettingParser(new[] { "Power.AdcMultiplierOverride=2.5" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.All(p => p.Setting.Name == "AdcMultiplierOverride").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Section.Name == "Power").ShouldBeTrue();
        result.ParsedSettings.All(p => p.Value is float f && f == 2.5f).ShouldBeTrue();

        result.ValidationIssues.ShouldBeEmpty();
    }
}
