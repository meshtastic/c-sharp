using Microsoft.Extensions.Logging;
using Meshtastic.Data;

namespace Meshtastic.Cli.Commands;

public class FactoryResetCommandHandler : DeviceCommandHandler
{
    public FactoryResetCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Factory reseting device...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateFactoryResetMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}
