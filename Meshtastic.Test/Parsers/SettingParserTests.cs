using Meshtastic.Cli.Parsers;

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
        result.ParsedSettings.Should().AllSatisfy(p => p.Value.Should().Be("true"));

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
}
