using Meshtastic.Protobufs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Meshtastic.Cli.Serialization;

public class FilteredTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector inner;

    public FilteredTypeInspector(ITypeInspector inner)
    {
        this.inner = inner;
    }

    public override string GetEnumName(Type enumType, string name) => inner.GetEnumName(enumType, name);

    public override string GetEnumValue(object enumValue) => inner.GetEnumValue(enumValue);

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        var properties = inner.GetProperties(type, container)
            .Where(p => !p.Name.Equals("Version", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Name.StartsWith("Has", StringComparison.OrdinalIgnoreCase));
            
        return properties;
    }
}