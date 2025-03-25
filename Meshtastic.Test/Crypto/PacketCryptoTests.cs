using Meshtastic.Crypto;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Crypto;

public class PacketCryptoTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestDefaultKeyDecrypt()
    {
        //{ "packet": { "from": 4202784164, "to": 4294967295, "channel": 8, "encrypted": "kiDV39nDDsi8AON+Czei6zUpy+F/7E+lyIpicxJR40KXBFmPkqFUEnobI5voQadha+s=", "id": 1777428186, "hopLimit": 3, "priority": "BACKGROUND", "hopStart": 3 }, "channelId": "LongFast", "gatewayId": "!fa8165a4" }
        var nonce = new NonceGenerator(4202784164, 1777428186).Create();

        var decrypted = PacketEncryption.TransformPacket(Convert.FromBase64String("kiDV39nDDsi8AON+Czei6zUpy+F/7E+lyIpicxJR40KXBFmPkqFUEnobI5voQadha+s="), nonce, Resources.DEFAULT_PSK);
        var testMessage = Meshtastic.Protobufs.Data.Parser.ParseFrom(decrypted);
        testMessage.Portnum.Should().Be(PortNum.NodeinfoApp);
        var nodeInfo = User.Parser.ParseFrom(testMessage.Payload);
        nodeInfo.LongName.Should().Be("Meshtastic 65a4");
    }
}