using Meshtastic.Protobufs;

namespace Meshtastic.Data;

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
}