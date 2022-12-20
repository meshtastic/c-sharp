using Meshtastic.Connections;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;
public class InfoCommand : Command
{
    public InfoCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var infoCommandHandler = new InfoCommandHandler();
        this.SetHandler(infoCommandHandler.Handle,
            new ConnectionBinder(port, host),
            new LoggingBinder());
    }
}
public class InfoCommandHandler : DeviceCommandHandler
{
    public async Task Handle(DeviceConnectionContext context, ILogger logger)
    {
        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
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
