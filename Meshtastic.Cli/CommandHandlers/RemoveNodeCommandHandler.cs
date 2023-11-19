using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class RemoveNodeCommandHandler : DeviceCommandHandler
{
    private readonly uint nodeNum;

    public RemoveNodeCommandHandler(uint nodeNum, DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) 
        {
        this.nodeNum = nodeNum;
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
        if (!container.Nodes.Any(n => n.Num == nodeNum))
        {
            Logger.LogError($"Node with nodenum {nodeNum} not found in device's NodeDB");
            return;
        }

        Logger.LogInformation("Removing device from NodeDB..");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateRemoveByNodenumMessage(nodeNum);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}