using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Data;

[TestFixture]
public class ToRadioMessageFactoryTests
{
    private ToRadioMessageFactory factory;

    [SetUp]
    public void Setup()
    {
        factory = new ToRadioMessageFactory();
    }

    [Test]
    public void CreateMeshPacketMessage_Should_ReturnValidMeshPacket()
    {
        var result = factory.CreateMeshPacketMessage(new MeshPacket()
        {
        });
        result.Packet.ShouldNotBeNull();
    }

    [Test]
    public void CreateWantConfigMessage_Should_ReturnValidWantConfig()
    {
        var result = factory.CreateWantConfigMessage();
        result.WantConfigId.ShouldBeGreaterThan((uint)0);
    }

    [Test]
    public void CreateXmodemPacketMessage_Should_ReturnValidXModemMessage()
    {
        var result = factory.CreateXmodemPacketMessage();
        result.XmodemPacket.ShouldNotBeNull();
    }
}
