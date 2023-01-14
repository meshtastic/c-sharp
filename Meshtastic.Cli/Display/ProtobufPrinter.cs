using Google.Protobuf;
using Google.Protobuf.Collections;
using Meshtastic.Cli;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Extensions;
using Meshtastic.Cli.Parsers;
using Meshtastic.Data;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using QRCoder;
using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Display;

[ExcludeFromCodeCoverage]
public class ProtobufPrinter
{
    private readonly DeviceStateContainer container;
    private readonly OutputFormat outputFormat;

    public ProtobufPrinter(DeviceStateContainer container, OutputFormat outputFormat)
    {
        this.container = container;
        this.outputFormat = outputFormat;
    }

    public void Print()
    {
        if (outputFormat == OutputFormat.PrettyConsole)
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
                (node.Position?.LatitudeI * 1e-7).ToString() ?? String.Empty,
                (node.Position?.LongitudeI * 1e-7).ToString() ?? String.Empty,
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

        foreach (var channel in channels)
        {
            if (channel == null)
                continue;

            table.AddRow(channel.Index.ToString(),
                channel.Settings.Name,
                channel.Role.ToString(),
                channel.Settings.Psk.IsEmpty ? String.Empty : Convert.ToBase64String(channel.Settings.Psk.ToByteArray()),
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
        if (outputFormat == OutputFormat.PrettyConsole)
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

    public void PrintUrl()
    {
        if (outputFormat == OutputFormat.PrettyConsole)
        {
            var channelSet = new ChannelSet()
            {
                LoraConfig = container.LocalConfig.Lora
            };
            container.Channels.ForEach(channel =>
            {
                channelSet.Settings.Add(channel.Settings);
            });
            var serialized = channelSet.ToByteArray();
            var base64 = Convert.ToBase64String(serialized);
            base64 = base64.Replace("-", String.Empty).Replace('+', '-').Replace('/', '_');
            var url = $"https://meshtastic.org/e/#{base64}";
            AnsiConsole.MarkupLine(url);

            QRCodeGenerator qrGenerator = new();
            QRCodeData data = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            AsciiQRCode qrCode = new(data);
            string qrCodeAsAsciiArt = qrCode.GetGraphic(1);
            AnsiConsole.Write(qrCodeAsAsciiArt);
        }
    }

    public void PrintMetadata(DeviceMetadata metadata)
    {
        if (outputFormat == OutputFormat.PrettyConsole)
        {
            var table = new Table();
            table.Expand();
            table.BorderColor(StyleResources.MESHTASTIC_GREEN);
            table.RoundedBorder();
            table.AddColumns("Name", "Value");

            table.AddRow(nameof(metadata.FirmwareVersion), metadata.FirmwareVersion.ToString());
            table.AddRow(nameof(metadata.DeviceStateVersion), metadata.DeviceStateVersion.ToString());
            table.AddRow(nameof(metadata.HasBluetooth), metadata.HasBluetooth.ToString());
            table.AddRow(nameof(metadata.HasWifi), metadata.HasWifi.ToString());
            table.AddRow(nameof(metadata.HasEthernet), metadata.HasEthernet.ToString());
            table.AddRow(nameof(metadata.CanShutdown), metadata.CanShutdown.ToString());
            AnsiConsole.Write(table);
        }
    }

    public void PrintRoute(RepeatedField<uint> route)
    {
        if (outputFormat == OutputFormat.PrettyConsole)
        {
            var root = new Tree("[bold]Route[/]")
            {
                Style = new Style(StyleResources.MESHTASTIC_GREEN)
            };

            IHasTreeNodes currentTreeNode = root;
            foreach (var nodeNum in route)
            {
                var node = container.Nodes.Find(n => n.Num == nodeNum);
                var content = $"Position: {node?.Position.ToDisplayString()}{Environment.NewLine}";
                var panel = new Panel(content)
                {
                    Header = new PanelHeader(container.GetNodeDisplayName(nodeNum))
                };
                currentTreeNode = currentTreeNode.AddNode(panel);
            }
            AnsiConsole.Write(root);
        }
    }
}