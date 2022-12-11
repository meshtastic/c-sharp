using System.Reflection;

namespace Meshtastic.Cli.Reflection;

public static class ReflectionExtensions
{
    private static string[] Exclusions => new[] {
        "Version", "Parser", "Descriptor",
        "Name", "ClrType", "ContainingType",
        "Fields", "Extensions", "NestedTypes",
        "EnumTypes", "Oneofs", "RealOneofCount",
        "FullName", "File", "Declaration",
        "IgnoreIncoming"
    };

    public static IEnumerable<string> GetSettingsOptions<TSettings>(this TSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        return GetFields(settings.GetType())
            .SelectMany(section => GetFields(section).Select(pref => $"{section.Name}.{pref.Name}"));
    }

    public static FieldInfo? FindFieldByName(this Type type, string name)
    {
        var fields = type.GetFields();
        return fields.FirstOrDefault(field => String.Equals(field.Name, name.Trim(), StringComparison.InvariantCultureIgnoreCase));
    }
    public static PropertyInfo? FindPropertyByName(this object instance, string name) =>
      GetProperties(instance)
      .FirstOrDefault(prop => String.Equals(prop.Name, name.Trim(), StringComparison.InvariantCultureIgnoreCase));

    public static PropertyInfo? FindPropertyByName(this Type type, string name) =>
        GetProperties(type)
        .FirstOrDefault(prop => String.Equals(prop.Name, name.Trim(), StringComparison.InvariantCultureIgnoreCase));

    public static IEnumerable<FieldInfo> GetFields(this Type type)
    {
        return type
            .GetFields()
            .Where(p => !Exclusions.Contains(p.Name));
    }

    public static IEnumerable<FieldInfo> GetFields(this object instance)
    {
        return instance
            .GetType()
            .GetFields()
            .Where(p => !Exclusions.Contains(p.Name));
    }

    public static IEnumerable<PropertyInfo> GetProperties(this Type type)
    {
        return type
            .GetProperties()
            .Where(p => !Exclusions.Contains(p.Name));
    }

    public static IEnumerable<PropertyInfo> GetProperties(this object instance)
    { 
        return instance
            .GetType()
            .GetProperties()
            .Where(p => !Exclusions.Contains(p.Name));
    }

    public static string GetSettingValue(this FieldInfo field, object instance) =>
        (field.GetValue(instance)?.ToString() ?? string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);

    public static string GetSettingValue(this PropertyInfo property, object instance) =>
        (property.GetValue(instance)?.ToString() ?? string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);

}