using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class DfuCommandHandler : DeviceCommandHandler
{
    public DfuCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
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
        Logger.LogInformation($"Attempting to enter DFU mode (only works on NRF52 devices)...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateEnterDfuMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}
