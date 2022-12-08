using System.Reflection;
using Meshtastic.Protobufs;
using Spectre.Console;
using Spectre.Console.Rendering;

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
            "FullName", "File", "Declaration",
            "IgnoreIncoming"
        };
        return instance
            .GetType()
            .GetProperties()
            .Where(p => !exclusions.Contains(p.Name));
    }

    public void Print()
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(
            PrintMyNodeInfo(this.myNodeInfo),
            PrintChannels(this.channels),
            PrintConfig(this.localConfig, "Config"), 
            PrintConfig(this.localModuleConfig, "Module Config"));
        AnsiConsole.Write(grid);
    }

    private string GetSettingValue(PropertyInfo property, object instance) =>
        (property.GetValue(instance)?.ToString() ?? String.Empty).Replace("[", String.Empty).Replace("]", String.Empty);

    private Tree PrintChannels(List<Channel> channels)
    {
        var root = new Tree("Channels");
        root.Style = new Style(Resources.MESHTASTIC_GREEN);

        var table = new Table();
        table.Expand();
        table.BorderColor(Resources.MESHTASTIC_GREEN);
        table.RoundedBorder();
        table.AddColumns("#", "Name", "Role", "PSK", "Uplink", "Downlink");

        foreach (var channel in channels) {
            if (channel == null)
                continue;
      
            table.AddRow(channel.Index.ToString(), 
                channel.Settings.Name, 
                channel.Role.ToString(), 
                channel.Settings.Psk.IsEmpty ? String.Empty : BitConverter.ToString(channel.Settings.Psk.ToByteArray()), 
                channel.Settings.UplinkEnabled.ToString(),
                channel.Settings.DownlinkEnabled.ToString());
        }
        
        root.AddNode(table);
        return root;
    }
    private Tree PrintMyNodeInfo(MyNodeInfo myNodeInfo)
    {
        var root = new Tree("My Node Info");
        root.Style = new Style(Resources.MESHTASTIC_GREEN);

        var table = new Table();
        table.Expand();
        table.BorderColor(Resources.MESHTASTIC_GREEN);
        table.RoundedBorder();
        table.AddColumns("Setting", "Value");
        foreach (var property in GetProperties(myNodeInfo))
        {
            if (property == null)
                continue;
            
            table.AddRow(property.Name, GetSettingValue(property, myNodeInfo));
        }
        root.AddNode(table);

        return root;
    }

    private Tree PrintConfig(object config, string name)
    {
        var root = new Tree(name);
        root.Style = new Style(Resources.MESHTASTIC_GREEN);
        var sectionValues = new List<string>();
        foreach (var sectionInfo in GetProperties(config))
        {
            var section = sectionInfo.GetValue(config);
            if (section == null)
                continue;

            var sectionNode = root.AddNode(sectionInfo.Name);
            var table = new Table();
            table.Expand();
            table.BorderColor(Resources.MESHTASTIC_GREEN);
            table.RoundedBorder();
            table.AddColumns("Setting", "Value");

            GetProperties(section!).ToList().ForEach(prop =>
            {
                table.AddRow(prop.Name, GetSettingValue(prop, section));
            });
            sectionNode.AddNode(table);
        }
        return root;
    }
}
