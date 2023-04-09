using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class GetCannedMessagesCommandHandler : DeviceCommandHandler
{
    public GetCannedMessagesCommandHandler(DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext)
    { }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting canned messages from device...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateGetCannedMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            (fromRadio, container) =>
            {
                var adminMessage = fromRadio.GetPayload<AdminMessage>();
                if (adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.GetCannedMessageModuleMessagesResponse)
                {
                    Logger.LogInformation($"Canned messages: {adminMessage?.GetCannedMessageModuleMessagesResponse}");
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
