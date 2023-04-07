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
using Spectre.Console.Rendering;
using System.Buffers.Binary;
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
            AnsiConsole.Write(PrintNodeInfos());
        }
    }

    public Tree PrintNodeInfos()
    {
        var root = new Tree("[bold]Nodes[/]")
        {
            Style = new Style(StyleResources.MESHTASTIC_GREEN)
        };
        root.AddNode(PrintNodesTable());
        return root;
    }

    public Table PrintNodesTable(bool compactTable = false)
    {
        var table = new Table();
        table.Expand();
        table.Border(compactTable ? TableBorder.Minimal : TableBorder.Rounded);
        table.BorderColor(StyleResources.MESHTASTIC_GREEN);
        table.Centered();
        if (compactTable)
            table.AddColumns("Name", "Latitude", "Longitude", "Battery", "SNR", "Last Heard");
        else
            table.AddColumns("ID#", "Name", "Latitude", "Longitude", "Battery", "Air Util", "Ch. Util", "SNR", "Last Heard");

        foreach (var node in container.Nodes)
        {
            if (compactTable)
            {
                table.AddRow(
                    new Text(container.GetNodeDisplayName(node.Num, hideNodeNum: true), new Style(GetColorFromNum(node.Num))),
                    new Text((node.Position?.LatitudeI * 1e-7 ?? 0).ToString("N6") ?? String.Empty),
                    new Text((node.Position?.LongitudeI * 1e-7 ?? 0).ToString("N6") ?? String.Empty),
                    new Text($"{node.DeviceMetrics?.BatteryLevel}%"),
                    new Text(node.Snr.ToString()),
                    new Text(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(node.LastHeard).AsTimeAgo()));
            }
            else
            {
                table.AddRow(node.Num.ToString(),
                    container.GetNodeDisplayName(node.Num),
                    (node.Position?.LatitudeI * 1e-7 ?? 0).ToString("N6") ?? String.Empty,
                    (node.Position?.LongitudeI * 1e-7 ?? 0).ToString("N6") ?? String.Empty,
                    $"{node.DeviceMetrics?.BatteryLevel}%",
                    $"{node.DeviceMetrics?.AirUtilTx.ToString("N2")}%" ?? String.Empty,
                    $"{node.DeviceMetrics?.ChannelUtilization.ToString("N2")}%" ?? String.Empty,
                    node.Snr.ToString(),
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(node.LastHeard).AsTimeAgo());
            }
        }
        return table;
    }

    public Panel PrintNodesPanel()
    {
        return new Panel(PrintNodesTable(true))
        {
            Header = new PanelHeader("Nodes"),
            Expand = true,
            BorderStyle = new Style(StyleResources.MESHTASTIC_GREEN)
        };
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

            table.AddRow(nameof(metadata.HwModel), metadata.HwModel.ToString());
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

    public Panel PrintTrafficCharts()
    {
        var myInfo = container.Nodes.FirstOrDefault(n => n.Num == container.MyNodeInfo.MyNodeNum);
        var airTimeStats = myInfo != null ? $"Channel Utilization {myInfo.DeviceMetrics.ChannelUtilization:N2}% / Airtime {myInfo.DeviceMetrics.AirUtilTx:N2}%" : String.Empty;

        var byNodeChart = new BreakdownChart()
           .FullSize();
        
        foreach(var node in container.Nodes.Select(n => new { n.Num, Name = container.GetNodeDisplayName(n.Num, hideNodeNum: true), Count = container.FromRadioMessageLog.Count(fr => fr.Packet?.From == n.Num) }))
        {
            byNodeChart.AddItem(node.Name, node.Count, GetColorFromNum(node.Num));
        }

        var charts = new Rows(new Text(String.Empty),
            new Text(airTimeStats).Centered(),
            new Text(String.Empty),
            new Text("By Port:").Centered(),
            new BarChart()
                .AddItem("Admin", GetMessageCountByPortNum(PortNum.AdminApp), Color.Red)
                .AddItem("Text", GetMessageCountByPortNum(PortNum.TextMessageApp) + GetMessageCountByPortNum(PortNum.TextMessageCompressedApp), Color.Yellow)
                .AddItem("Routing", GetMessageCountByPortNum(PortNum.RoutingApp), Color.Grey)
                .AddItem("Position", GetMessageCountByPortNum(PortNum.PositionApp), Color.Green)
                .AddItem("Waypoint", GetMessageCountByPortNum(PortNum.WaypointApp), Color.Pink1)
                .AddItem("NodeInfo", GetMessageCountByPortNum(PortNum.NodeinfoApp), Color.White)
                .AddItem("Telemetry", GetMessageCountByPortNum(PortNum.TelemetryApp, ignoreLocal: true), Color.Blue),
            new Text(String.Empty),
            new Text("By Node:").Centered(),
            new Text(String.Empty),
            byNodeChart);

        var panel = new Panel(charts)
        {
            Header = new PanelHeader($"Mesh traffic"),
            Expand = true,
            BorderStyle = new Style(StyleResources.MESHTASTIC_GREEN)
        };
        return panel;
    }

    public Panel PrintMessagesPanel()
    {
        var texts = container.FromRadioMessageLog
            .Where(fr => fr.Packet?.Decoded?.Portnum == PortNum.TextMessageApp || fr.Packet?.Decoded?.Portnum == PortNum.TextMessageCompressedApp)
            .Select(fr => new
            {
                time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(fr.Packet.RxTime).AsTimeAgo(),
                num = fr.Packet.From,
                from = $"[bold]{container.GetNodeDisplayName(fr.Packet.From, hideNodeNum: true)}[/]",
                message = fr.Packet?.Decoded?.Payload.ToStringUtf8()
            })
            .SelectMany(text =>
            {
                return new List<IRenderable>() {
                    new Rule($"{text.from} - {text.time}")
                    {
                        Style = new Style(GetColorFromNum(text.num))
                    }, 
                    new Text(text.message!)
                };
            });

        if (!texts.Any())
        {
            return new Panel("Messages")
            {
                Expand = true,
                BorderStyle = new Style(StyleResources.MESHTASTIC_GREEN)
            };
        }

        var messages = new Rows(texts);
        var panel = new Panel(messages)
        {
            Header = new PanelHeader("Messages"),
            Expand = true,
            BorderStyle = new Style(StyleResources.MESHTASTIC_GREEN)
        };
        return panel;
    }

    private static Color GetColorFromNum(uint num)
    {
        var output = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(output, (int)num);
        return new Color(WrapAround(output[2]), WrapAround(output[1]), WrapAround(output[0])); 
    }
    
    private static byte WrapAround(byte val)
    {
        return val;
    }

    private int GetMessageCountByPortNum(PortNum portNum, bool ignoreLocal = false)
    {
        return container.FromRadioMessageLog.Count(fr => fr.Packet?.Decoded?.Portnum == portNum && (!ignoreLocal || fr.Packet?.From != container.MyNodeInfo.MyNodeNum));
    }
}