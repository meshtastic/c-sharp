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
        //await AnsiConsole.Live(table)
        // .StartAsync(async ctx =>
        // {
        //     await Connection.ReadFromRadio((fromRadio, container) =>
        //     {
        //         var routeDiscovery = fromRadio.GetMessage<RouteDiscovery>();
        //         var table = new Table().Centered();

        //         return Task.FromResult(false);
        //     });

        //     table.AddColumn("Bar");
        //     ctx.Refresh();
        //     Thread.Sleep(1000);
        // });

    }
}
