using Meshtastic.Data;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class GetCannedMessagesCommandHandler : DeviceCommandHandler
{

    public GetCannedMessagesCommandHandler(DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext) { }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting canned messages from device...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var adminMessage = adminMessageFactory.CreateGetCannedMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            (fromDevice, container) =>
            {
                if (fromDevice?.ParsedMessage.adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.GetCannedMessageModuleMessagesResponse)
                {
                    Logger.LogInformation($"Canned messages: {fromDevice?.ParsedMessage.adminMessage.GetCannedMessageModuleMessagesResponse}");
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
    }
}
