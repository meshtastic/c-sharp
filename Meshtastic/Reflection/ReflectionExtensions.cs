using Google.Protobuf;
using Meshtastic.Protobufs;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

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

    public static IEnumerable<string> GetSettingsOptions<TSettings>(this TSettings settings) where TSettings : IMessage
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        return settings.Descriptor.Fields.InFieldNumberOrder()
            .Where(s => s.Name != "version")
            .SelectMany(section =>
            {
                return section.MessageType.Fields.InFieldNumberOrder()
                    .Select(setting => $"{section.PropertyName}.{setting.PropertyName}");
            });

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

    public static string GetSettingValue(this PropertyInfo property, object instance) =>
        (property.GetValue(instance)?.ToString() ?? string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);

}