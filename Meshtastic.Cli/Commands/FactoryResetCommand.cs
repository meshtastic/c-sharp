using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;

public class FactoryResetCommand : Command
{
    public FactoryResetCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var handler = new FactoryResetCommandHandler();
        this.SetHandler(handler.Handle,
            new ConnectionBinder(port, host),
            new LoggingBinder());
    }
}
public class FactoryResetCommandHandler : DeviceCommandHandler
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
        AnsiConsole.WriteLine("Getting device metadata...");
        var adminMessageFactory = new AdminMessageFactory(container);
        var adminMessage = adminMessageFactory.CreateFactoryResetMessage();
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage).ToByteArray(), 
            (fromDevice, container) =>
            {
                return Task.FromResult(fromDevice != null);
            });
    }
}
