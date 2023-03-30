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
                new Layout("Left"),
                new Layout("Right")
                    .SplitRows(
                        new Layout("Top"),
                        new Layout("Bottom")));
        await AnsiConsole.Live(layout)
            .StartAsync(async ctx =>
            {
                await Connection.ReadFromRadio((fromRadio, container) =>
                {
                    var printer = new ProtobufPrinter(container, OutputFormat);
                    layout["Bottom"].Update(printer.PrintTrafficChart());
                    layout["Top"].Update(printer.PrintNodeInfos(container.Nodes));
                    ctx.Refresh();

                    return Task.FromResult(false);
                });
            });
    }
}
