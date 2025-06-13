using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using static Meshtastic.Protobufs.AdminMessage.Types;

namespace Meshtastic.Cli.CommandHandlers;

public class SendInputEventCommandHandler(uint eventCode,
    uint? kbChar,
    uint? touchX,
    uint? touchY,
    DeviceConnectionContext context,
    CommandContext commandContext) : DeviceCommandHandler(context, commandContext)
{
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var adminMessage = GetAdminMessage(container, GetInputEvent());
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        // Uncomment the following lines to send some additional input events for testing
        // adminMessage = GetAdminMessage(container, GetInputEvent(17)); // INPUT_BROKER_UP
        // await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        // adminMessage = GetAdminMessage(container, GetInputEvent(18)); // INPUT_BROKER_DOWN
        // await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        // adminMessage = GetAdminMessage(container, GetInputEvent(24)); // INPUT_BROKER_BACK
        // await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        // adminMessage = GetAdminMessage(container, GetInputEvent(28)); // INPUT_BROKER_PRESS
        // await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        // adminMessage = GetAdminMessage(container, GetInputEvent(28)); // INPUT_BROKER_PRESS
        // await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
    }

    private MeshPacket GetAdminMessage(DeviceStateContainer container, InputEvent inputEvent)
    {
        Logger.LogInformation($"Sending input event (code: {inputEvent.EventCode}) to device...");
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        return adminMessageFactory.CreateSendInputEventMessage(inputEvent);
    }

    private InputEvent GetInputEvent(uint? overrideEventCode = null)
    {
        var inputEvent = new InputEvent()
        {
            EventCode = overrideEventCode ?? eventCode
        };
        if (kbChar.HasValue)
            inputEvent.KbChar = kbChar.Value;
        if (touchX.HasValue)
            inputEvent.TouchX = touchX.Value;
        if (touchY.HasValue)
            inputEvent.TouchY = touchY.Value;
        return inputEvent;
    }

    /*
        // Input broker event codes from firmware
        enum input_broker_event {
            INPUT_BROKER_NONE = 0,
            INPUT_BROKER_SELECT = 10,
            INPUT_BROKER_UP = 17,
            INPUT_BROKER_DOWN = 18,
            INPUT_BROKER_LEFT = 19,
            INPUT_BROKER_RIGHT = 20,
            INPUT_BROKER_CANCEL = 24,
            INPUT_BROKER_BACK = 27,
            INPUT_BROKER_USER_PRESS,
            INPUT_BROKER_SHUTDOWN = 0x9b,
            INPUT_BROKER_GPS_TOGGLE = 0x9e,
            INPUT_BROKER_SEND_PING = 0xaf,
            INPUT_BROKER_MATRIXKEY = 0xFE,
            INPUT_BROKER_ANYKEY = 0xff
        };

        #define INPUT_BROKER_MSG_BRIGHTNESS_UP 0x11
        #define INPUT_BROKER_MSG_BRIGHTNESS_DOWN 0x12
        #define INPUT_BROKER_MSG_REBOOT 0x90
        #define INPUT_BROKER_MSG_MUTE_TOGGLE 0xac
        #define INPUT_BROKER_MSG_FN_SYMBOL_ON 0xf1
        #define INPUT_BROKER_MSG_FN_SYMBOL_OFF 0xf2
        #define INPUT_BROKER_MSG_BLUETOOTH_TOGGLE 0xAA
        #define INPUT_BROKER_MSG_TAB 0x09
        #define INPUT_BROKER_MSG_EMOTE_LIST 0x8F
    */
}
