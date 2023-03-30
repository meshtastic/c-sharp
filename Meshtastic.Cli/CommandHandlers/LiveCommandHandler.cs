using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Meshtastic.Cli.CommandHandlers;

public class LiveCommandHandler : DeviceCommandHandler
{
    public LiveCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }

    public async Task<DeviceStateContainer> Handle()
    {
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
        layout["Nodes"].Update(printer.PrintNodesTable(compactTable: true));
        layout["Traffic"].Update(printer.PrintTrafficChart());
        layout["Messages"].Update(printer.PrintMessagesPanel());
    }
}
