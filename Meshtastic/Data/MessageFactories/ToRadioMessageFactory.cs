using Google.Protobuf;
using Meshtastic.Protobufs;
using static Meshtastic.Protobufs.XModem.Types;

namespace Meshtastic.Data.MessageFactories;

public class ToRadioMessageFactory
{
    public ToRadioMessageFactory()
    {
    }

    public ToRadio CreateWantConfigMessage() =>
        new()
        {
            WantConfigId = (uint)Random.Shared.Next(),
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

    public ToRadio CreateMqttClientProxyMessage(string topic, byte[] payload) =>
        new()
        {
            MqttClientProxyMessage = new MqttClientProxyMessage()
            {
                Topic = topic,
                Data = ByteString.CopyFrom(payload)
            }
        };
}