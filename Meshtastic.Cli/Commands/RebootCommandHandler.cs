using Meshtastic.Data;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class RebootCommandHandler : DeviceCommandHandler
{
    private readonly bool isOtaMode = false;
    private readonly int seconds = 5;

    public RebootCommandHandler(bool isOtaMode,
        int seconds,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        this.isOtaMode = isOtaMode;
        this.seconds = seconds;
    }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation($"Rebooting in {seconds} seconds...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateRebootMessage(seconds, isOtaMode);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}
