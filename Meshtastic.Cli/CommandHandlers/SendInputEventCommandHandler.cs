using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class SendInputEventCommandHandler : DeviceCommandHandler
{
    private readonly uint eventCode;
    private readonly uint? kbChar;
    private readonly uint? touchX;
    private readonly uint? touchY;

    public SendInputEventCommandHandler(uint eventCode,
        uint? kbChar,
        uint? touchX,
        uint? touchY,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        this.eventCode = eventCode;
        this.kbChar = kbChar;
        this.touchX = touchX;
        this.touchY = touchY;
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
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        
        var inputEvent = new AdminMessage.Types.InputEvent()
        {
            EventCode = eventCode
        };

        if (kbChar.HasValue)
            inputEvent.KbChar = kbChar.Value;

        if (touchX.HasValue)
            inputEvent.TouchX = touchX.Value;

        if (touchY.HasValue)
            inputEvent.TouchY = touchY.Value;

        Logger.LogInformation($"Sending input event (code: {eventCode}) to device...");
        var adminMessage = adminMessageFactory.CreateSendInputEventMessage(inputEvent);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }
}
