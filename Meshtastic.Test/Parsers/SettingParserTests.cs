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

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("Enabled"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Mqtt"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().BeNull());

        result.ValidationIssues.Should().BeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidModuleConfigArgs_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.enabled=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("Enabled"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Mqtt"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().Be(true));

        result.ValidationIssues.Should().BeEmpty();
    }

    //[Test]
    //public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenIntToImplicitBoolean_Set()
    //{
    //    var parser = new SettingParser(new[] { "mqtt.enabled=1" });
    //    var result = parser.ParseSettings(isGetOnly: false);

    //    result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("Enabled"));
    //    result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Mqtt"));
    //    result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().Be(true));

    //    result.ValidationIssues.Should().BeEmpty();
    //}

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenEnumValue_Set()
    {
        var parser = new SettingParser(new[] { "serial.baud=Baud19200" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("Baud"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Serial"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().Be(Serial_Baud.Baud19200));

        result.ValidationIssues.Should().BeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenValidConfigArgs_Get()
    {
        var parser = new SettingParser(new[] { "display.screenOnSecs" });
        var result = parser.ParseSettings(isGetOnly: true);

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("ScreenOnSecs"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Display"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().BeNull());

        result.ValidationIssues.Should().BeEmpty();
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenBardArgs_Set()
    {
        var parser = new SettingParser(new[] { "screenOnSecs" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenBadArgs_Set()
    {
        var parser = new SettingParser(new[] { "screenOnSecs=42" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenMissingSection_Set()
    {
        var parser = new SettingParser(new[] { "doesntexist.enabled" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenMissingSection_Set()
    {
        var parser = new SettingParser(new[] { "doesntexist.enabled=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }


    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenMissingSetting_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.derp" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnValidationIssues_GivenMissingSetting_Set()
    {
        var parser = new SettingParser(new[] { "mqtt.derp=true" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().BeEmpty();
        result.ValidationIssues.Should().HaveCount(1);
    }

    [Test]
    public void ParseSettings_Should_ReturnResultWithPropertyInfo_GivenSnakeCase()
    {
        var parser = new SettingParser(new[] { "display.screen_on_secs" });
        var result = parser.ParseSettings(isGetOnly: true);

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("ScreenOnSecs"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Display"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().BeNull());

        result.ValidationIssues.Should().BeEmpty();
    }

    [Test]
    public void ParseSettings_Should_SetFloatValue()
    {
        var parser = new SettingParser(new[] { "Power.AdcMultiplierOverride=2.5" });
        var result = parser.ParseSettings(isGetOnly: false);

        result.ParsedSettings.Should().AllSatisfy(p => p.Setting.Name.Should().BeEquivalentTo("AdcMultiplierOverride"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Section.Name.Should().BeEquivalentTo("Power"));
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().BeEquivalentTo(2.5));

        result.ValidationIssues.Should().BeEmpty();
    }
}
