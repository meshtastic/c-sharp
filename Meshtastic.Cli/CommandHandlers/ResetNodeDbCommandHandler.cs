using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class ResetNodeDbCommandHandler : DeviceCommandHandler
{
    public ResetNodeDbCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Reseting device node db...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateNodeDbResetMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}