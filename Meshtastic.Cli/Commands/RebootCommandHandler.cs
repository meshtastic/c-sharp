using Meshtastic.Connections;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;

public class RebootCommand : Command
{
    public RebootCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var rebootCommandHandler = new RebootCommandHandler();
        this.SetHandler(rebootCommandHandler.Handle,
            new ConnectionBinder(port, host),
            new LoggingBinder());
    }
}
public class RebootCommandHandler : DeviceCommandHandler
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

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        AnsiConsole.WriteLine("Rebooting in 10 seconds...");
        var adminMessageFactory = new AdminMessageFactory(container);
        var adminMessage = adminMessageFactory.CreateRebootMessage();
        await connection!.WriteToRadio(adminMessage.ToByteArray(), AlwaysComplete);
    }
}
