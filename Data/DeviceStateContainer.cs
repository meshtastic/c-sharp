using System.Reflection;
using Meshtastic.Protobufs;
using Spectre.Console;

namespace Meshtastic.Data;

public class DeviceStateContainer 
{
    private LocalConfig localConfig;
    private LocalModuleConfig localModuleConfig;
    private List<Channel> channels;
    private MyNodeInfo myNodeInfo;
    private List<NodeInfo> nodes;

    public DeviceStateContainer(LocalConfig localConfig, 
        LocalModuleConfig localModuleConfig,
        List<Channel> channels,
        MyNodeInfo myNodeInfo,
        List<NodeInfo> nodes) 
    {
        this.localConfig = localConfig;
        this.localModuleConfig = localModuleConfig;
        this.channels = channels;
        this.myNodeInfo = myNodeInfo;
        this.nodes = nodes;
    }

    public DeviceStateContainer() 
    {
        this.localConfig = new LocalConfig();
        this.localModuleConfig = new LocalModuleConfig();
        this.channels = new List<Channel>();
        this.myNodeInfo = new MyNodeInfo();
        this.nodes = new List<NodeInfo>();
    } 

    private void SetConfig(Config.PayloadVariantOneofCase variant, Config config) 
    {
        var variantName = variant.ToString();
        var variantValue = typeof(Config).GetProperty(variantName)?.GetValue(config);
        typeof(LocalConfig).GetProperty(variantName)?.SetValue(this.localConfig, variantValue);
    }

    private void SetModuleConfig(ModuleConfig.PayloadVariantOneofCase variant, ModuleConfig config) 
    {
        var variantName = variant.ToString();
        var variantValue = typeof(ModuleConfig).GetProperty(variantName)?.GetValue(config);
        typeof(LocalModuleConfig).GetProperty(variantName)?.SetValue(this.localModuleConfig, variantValue);
    }

    public void AddFromRadio(FromRadio fromRadio) 
    {
        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Config) 
            SetConfig(fromRadio.Config.PayloadVariantCase, fromRadio.Config);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.ModuleConfig) 
            SetModuleConfig(fromRadio.ModuleConfig.PayloadVariantCase, fromRadio.ModuleConfig);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Channel)
            this.channels.Add(fromRadio.Channel);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.MyInfo)
            this.myNodeInfo = fromRadio.MyInfo;

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.NodeInfo)
            this.nodes.Add(fromRadio.NodeInfo);
    }

    private IEnumerable<PropertyInfo> GetProperties(object instance) 
    {
        var exclusions = new [] { 
            "Version", "Parser", "Descriptor", 
            "Name", "ClrType", "ContainingType", 
            "Fields", "Extensions", "NestedTypes",
            "EnumTypes", "Oneofs", "RealOneofCount",
            "Index", "FullName", "File",
            "Declaration", "IgnoreIncoming"
        };
        return instance
            .GetType()
            .GetProperties()
            .Where(p => !exclusions.Contains(p.Name));
    }

    public void Print()
    {
        PrintConfig(this.localConfig);
        PrintConfig(this.localModuleConfig);
    }

    private void PrintConfig(object config)
    {
        var table = new Table();
        table.BorderColor(Resources.MESHTASTIC_GREEN);

        var sectionValues = new List<string>();
        foreach (var sectionInfo in GetProperties(config))
        {
            var section = sectionInfo.GetValue(config);
            if (section == null)
                continue;

            table.AddColumn(sectionInfo.Name);

            var values = String.Join(Environment.NewLine, GetProperties(section!).Select(prop =>
            {
                //Console.WriteLine($"{prop.Name}: {prop.GetValue(section)}");
                return $"{prop.Name}: {prop.GetValue(section)}";
            }));
            sectionValues.Add(values);
        }
        table.AddRow(sectionValues.ToArray());
        AnsiConsole.Write(table);
    }
}
