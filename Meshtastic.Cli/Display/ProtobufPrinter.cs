using Meshtastic.Cli;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Reflection;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Display;

public class ProtobufPrinter
{
    private readonly DeviceStateContainer container;

    public ProtobufPrinter(DeviceStateContainer container)
    {
        this.container = container;
    }

    public void Print()
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(PrintMyNodeInfo(container.MyNodeInfo),
            PrintChannels(container.Channels));
        grid.AddRow(PrintConfig(container.LocalConfig, "[bold]Config[/]"), 
            PrintConfig(container.LocalModuleConfig, "[bold]Module Config[/]"));
        AnsiConsole.Write(grid);

        AnsiConsole.Write(PrintNodeInfos(container.Nodes));
    }

    public Tree PrintNodeInfos(List<NodeInfo> nodeInfos) 
    {
        var root = new Tree("[bold]Nodes[/]")
        {
            Style = new Style(StyleResources.MESHTASTIC_GREEN)
        };
        var table = new Table();
        table.Expand();
        table.BorderColor(StyleResources.MESHTASTIC_GREEN);
        table.RoundedBorder();
        table.AddColumns(nameof(NodeInfo.User.Id), nameof(NodeInfo.User.ShortName),
            nameof(NodeInfo.User.LongName), nameof(NodeInfo.Position.LatitudeI), nameof(NodeInfo.Position.LongitudeI),
            nameof(NodeInfo.DeviceMetrics.BatteryLevel), nameof(NodeInfo.DeviceMetrics.AirUtilTx), 
            nameof(NodeInfo.DeviceMetrics.ChannelUtilization), nameof(NodeInfo.Snr), nameof(NodeInfo.LastHeard));

        foreach (var node in nodeInfos)
        {
            table.AddRow(node.Num.ToString(),
                node.User.ShortName,
                node.User.LongName,
                node.Position?.LatitudeI.ToString() ?? String.Empty,
                node.Position?.LongitudeI.ToString() ?? String.Empty,
                node.DeviceMetrics?.BatteryLevel.ToString() ?? String.Empty,
                node.DeviceMetrics?.AirUtilTx.ToString() ?? String.Empty,
                node.DeviceMetrics?.ChannelUtilization.ToString() ?? String.Empty,
                node.Snr.ToString(),
                node.LastHeard.ToString());
        }
        root.AddNode(table);
        return root;
    }

    public Tree PrintChannels(List<Channel> channels)
    {
        var root = new Tree("[bold]Channels[/]")
        {
            Style = new Style(StyleResources.MESHTASTIC_GREEN)
        };

        var table = new Table();
        table.Expand();
        table.BorderColor(StyleResources.MESHTASTIC_GREEN);
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
        var root = new Tree("[bold]My Node Info[/]")
        {
            Style = new Style(StyleResources.MESHTASTIC_GREEN)
        };

        var table = new Table();
        table.Expand();
        table.BorderColor(StyleResources.MESHTASTIC_GREEN);
        table.RoundedBorder();
        table.AddColumns("Setting", "Value");
        foreach (var property in myNodeInfo.GetProperties())
        {
            if (property == null)
                continue;
            
            table.AddRow(property.Name, property.GetSettingValue(myNodeInfo));
        }
        root.AddNode(table);

        return root;
    }

    public Tree PrintConfig(object config, string name)
    {
        var root = new Tree($"[bold]{name}[/]")
        {
            Style = new Style(StyleResources.MESHTASTIC_GREEN)
        };
        var sectionValues = new List<string>();
        foreach (var sectionInfo in config.GetProperties())
        { 
            var section = sectionInfo.GetValue(config);
            if (section == null)
                continue;

            var sectionNode = root.AddNode(sectionInfo.Name);
            var table = new Table();
            table.Expand();
            table.BorderColor(StyleResources.MESHTASTIC_GREEN);
            table.RoundedBorder();
            table.HideHeaders();
            table.AddColumns("Setting", "Value");

            section!.GetProperties().ToList().ForEach(prop =>
            {
                table.AddRow(prop.Name, prop.GetSettingValue(section));
            });
            sectionNode.AddNode(table);
        }
        return root;
    }

    public void PrintSettings(IEnumerable<ParsedSetting> parsedSettings)
    { 
        var table = new Table();
        table.Expand();
        table.BorderColor(StyleResources.MESHTASTIC_GREEN);
        table.RoundedBorder();
        table.AddColumns("Setting", "Value");

        foreach (var setting in parsedSettings)
        {
            var instance = setting.Section.ReflectedType?.Name == nameof(this.container.LocalConfig) ?
                setting.Section.GetValue(this.container.LocalConfig) :
                setting.Section.GetValue(this.container.LocalModuleConfig);
            var value = setting.Setting.GetValue(instance);

            table.AddRow($"{setting.Section.Name}.{setting.Setting.Name}", value?.ToString() ?? String.Empty);
        }
        AnsiConsole.Write(table);
    }
}