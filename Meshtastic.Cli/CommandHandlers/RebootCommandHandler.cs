using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

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
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation($"Rebooting in {seconds} seconds...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateRebootMessage(seconds, isOtaMode);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}
