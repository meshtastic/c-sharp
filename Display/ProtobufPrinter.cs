using Meshtastic.Data;
using Meshtastic.Protobufs;
using Spectre.Console;

namespace Meshtastic.Display;

public static class ProtobufPrinter
{
    public static void Print(this DeviceStateContainer container)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(PrintMyNodeInfo(container.MyNodeInfo),
            PrintChannels(container.Channels),
            PrintConfig(container.LocalConfig, "Config"), 
            PrintConfig(container.LocalModuleConfig, "Module Config"));
        AnsiConsole.Write(grid);
    }

    public static Tree PrintChannels(List<Channel> channels)
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
    
    private static Tree PrintMyNodeInfo(MyNodeInfo myNodeInfo)
    {
        var root = new Tree("My Node Info");
        root.Style = new Style(Resources.MESHTASTIC_GREEN);

        var table = new Table();
        table.Expand();
        table.BorderColor(Resources.MESHTASTIC_GREEN);
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

    public static Tree PrintConfig(object config, string name)
    {
        var root = new Tree(name);
        root.Style = new Style(Resources.MESHTASTIC_GREEN);
        var sectionValues = new List<string>();
        foreach (var sectionInfo in config.GetProperties())
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

            section!.GetProperties().ToList().ForEach(prop =>
            {
                table.AddRow(prop.Name, prop.GetSettingValue(section));
            });
            sectionNode.AddNode(table);
        }
        return root;
    }
}