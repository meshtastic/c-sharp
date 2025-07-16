using Google.Protobuf;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.XModem.Types;

namespace Meshtastic.Data.MessageFactories;

public class ToRadioMessageFactory
{
    public ToRadioMessageFactory()
    {
    }

    // Create a ToRadio message with a empty payload
    public ToRadio CreateKeepAliveMessage() =>
       new()
       {
           Heartbeat = new Heartbeat()
       };

    public ToRadio CreateWantConfigMessage() =>
        new()
        {
            WantConfigId = (uint)Random.Shared.Next(),
        };

    public ToRadio CreateWantConfigOnlyMessage() =>
        new()
        {
            WantConfigId = 69420,
        };

    public ToRadio CreateWantConfigOnlyNodesMessage() =>
        new()
        {
            WantConfigId = 69421,
        };

    public ToRadio CreateMeshPacketMessage(MeshPacket packet) =>
        new()
        {
            Packet = packet
        };

    public ToRadio CreateXmodemPacketMessage(Control control = XModem.Types.Control.Stx) =>
        new()
        {
            XmodemPacket = new XModem()
            {
                Control = control
            }
        };

    public ToRadio CreateMqttClientProxyMessage(string topic, byte[] payload, bool retain = false) =>
        new()
        {
            MqttClientProxyMessage = new MqttClientProxyMessage()
            {
                Topic = topic,
                Data = ByteString.CopyFrom(payload),
                Retained = retain,
            }
        };
}