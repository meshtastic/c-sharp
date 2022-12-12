using Meshtastic.Connections;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;

namespace Meshtastic.Cli.Handlers;

public class InfoCommandHandler : DeviceCommandHandler
{
    public async Task Handle(DeviceConnectionContext context, ILogger logger)
    {
        await OnConnection(context, async () =>
        {
            var connection = context.GetDeviceConnection();
            var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container);
        printer.Print();
        return Task.CompletedTask;
    }
}
