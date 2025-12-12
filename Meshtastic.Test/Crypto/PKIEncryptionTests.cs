using System.Text;
using Google.Protobuf;
using Meshtastic.Crypto;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Crypto;
public class PKIEncryptionTests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void TestRealLifeDecryption()
	{
		var rawCapturedPacket = Convert.FromBase64String("DeBEYa4VoIfKoCo0hEfVe6AMHor2d8ciyda+ChJe/DROOMIPWyX8jL56DB3+6127ZQ4HQTGsfQPoS8Rl70e6ZjX2mbLXPeRBd2hFAABIwUgGUAFghP//////////AXgHiAEB");
		var recipientPrivateKey = Convert.FromBase64String("0FJd8sVxuyr3ee+n8ZVVxozvUbzGORwazxeR8D1iqmg=");
		var senderPublicKey = Convert.FromBase64String("WWb1Pvgu6zs1dnncqtMcLW6AvFo84U9pJfJp417i+T4=");
		var meshPacket = MeshPacket.Parser.ParseFrom(rawCapturedPacket);

		meshPacket.Decoded.ShouldBeNull();
		meshPacket.Encrypted.ShouldNotBeNull();
		PKIEncryption.Decrypt(recipientPrivateKey, senderPublicKey, meshPacket);
		meshPacket.Decoded.ShouldNotBeNull();
		(meshPacket.Encrypted?.Length ?? 0).ShouldBe(0);

		var plaintext = Encoding.UTF8.GetString(meshPacket.Decoded.Payload.ToArray());
		plaintext.ShouldBe("Test packet for PKI, please ignore");
	}

	[Test]
	public void EncryptDecryptRoundTrip()
	{
		var plaintext = "Test packet for PKI, please ignore";
		var recipientKeyPair = PKIEncryption.GenerateKeyPair();
		var senderKeyPair = PKIEncryption.GenerateKeyPair();
		var senderMeshPacket = new MeshPacket
		{
			From = 2242185114,
			Id = 1099381122,
			Decoded = new Protobufs.Data
			{
				Payload = ByteString.CopyFrom(Encoding.UTF8.GetBytes(plaintext))
			}
		};

		senderMeshPacket.Decoded.ShouldNotBeNull();
        (senderMeshPacket.Encrypted?.Length ?? 0).ShouldBe(0);
        PKIEncryption.Encrypt(senderKeyPair.privateKey, recipientKeyPair.publicKey, senderMeshPacket);
		senderMeshPacket.Decoded.ShouldBeNull();
		senderMeshPacket.Encrypted.ShouldNotBeNull();

		var recipientMeshPacket = MeshPacket.Parser.ParseFrom(senderMeshPacket.ToByteArray());

		recipientMeshPacket.Decoded.ShouldBeNull();
		recipientMeshPacket.Encrypted.ShouldNotBeNull();
		PKIEncryption.Decrypt(recipientKeyPair.privateKey, senderKeyPair.publicKey, recipientMeshPacket);
		recipientMeshPacket.Decoded.ShouldNotBeNull();
        (recipientMeshPacket.Encrypted?.Length ?? 0).ShouldBe(0);

        recipientMeshPacket.From.ShouldBe(senderMeshPacket.From);
		recipientMeshPacket.Id.ShouldBe(senderMeshPacket.Id);
		Encoding.UTF8.GetString(recipientMeshPacket.Decoded.Payload.ToByteArray()).ShouldBe(plaintext);
	}
}
