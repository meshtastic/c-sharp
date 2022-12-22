using System.Reflection;

namespace Meshtastic.Cli.Parsers;

public class Parser
{
    protected static object ParseValue(PropertyInfo setting, string value)
    {
        if (setting.PropertyType == typeof(uint))
            return uint.Parse(value);
        else if (setting.PropertyType == typeof(float))
            return float.Parse(value);
        else if (setting.PropertyType == typeof(bool))
            return bool.Parse(value);
        else if (setting.PropertyType == typeof(string))
            return value;
        else
            return Enum.Parse(setting.PropertyType!, value, ignoreCase: true);
    }
}

public record SettingParserResult(IEnumerable<ParsedSetting> ParsedSettings, IEnumerable<string> ValidationIssues);
public record ParsedSetting(PropertyInfo Section, PropertyInfo Setting, object? Value);