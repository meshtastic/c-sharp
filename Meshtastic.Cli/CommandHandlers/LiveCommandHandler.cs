using System.Net.Sockets;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.CommandHandlers;

public class LiveCommandHandler : DeviceCommandHandler
{
    private readonly TcpClient client = new();
    private NetworkStream? networkStream;
    public LiveCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
        await client.ConnectAsync("192.168.2.23", 8087);
        networkStream = client.GetStream();
        var buffer = new byte[networkStream.Length];
        await networkStream.ReadAsync(buffer);
        var message = dpp.cot.Message.Parse(buffer, 0, buffer.Length);
        AnsiConsole.WriteLine(message.ToXmlString());
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Messages"),
                new Layout("Right")
                    .SplitRows(
                        new Layout("Nodes"),
                        new Layout("Traffic")));

        await AnsiConsole.Live(layout)
            .StartAsync(async ctx =>
            {
                var printer = new ProtobufPrinter(container, OutputFormat);
                UpdateDashboard(layout, printer);

                await Connection.ReadFromRadio((fromRadio, container) =>
                {
                    printer = new ProtobufPrinter(container, OutputFormat);
                    UpdateDashboard(layout, printer);

                    ctx.Refresh();
                    return Task.FromResult(false);
                });
            });
    }
    

    private static void UpdateDashboard(Layout layout, ProtobufPrinter printer)
    {
        layout["Nodes"].Update(printer.PrintNodesPanel());
        layout["Traffic"].Update(printer.PrintTrafficCharts());
        layout["Messages"].Update(printer.PrintMessagesPanel());
    }
}
