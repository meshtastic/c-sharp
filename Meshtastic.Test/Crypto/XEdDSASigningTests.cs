using NUnit.Framework;
using Meshtastic.Crypto;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using System.Text;

namespace Meshtastic.Test.Crypto;

[TestFixture]
public class XEdDSASigningTests
{
    private byte[] _testPrivateKey = null!;
    private byte[] _testPublicKey = null!;

    [SetUp]
    public void Setup()
    {
        // Generate consistent test keys
        _testPrivateKey = Convert.FromHexString("1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef");
        _testPublicKey = Convert.FromHexString("abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890");
    }

    [Test]
    public void GenerateEdDSAKeysFromX25519_Should_ProduceValidKeys()
    {
        // Act
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Assert
        Assert.That(edPrivateKey, Is.Not.Null);
        Assert.That(edPrivateKey.Length, Is.EqualTo(32));
        Assert.That(edPublicKey, Is.Not.Null);
        Assert.That(edPublicKey.Length, Is.EqualTo(32));
        
        // Keys should not be all zeros
        Assert.That(edPrivateKey, Is.Not.All.EqualTo(0));
        Assert.That(edPublicKey, Is.Not.All.EqualTo(0));
    }

    [Test]
    public void ConvertX25519PublicKeyToEd25519_Should_ProduceValidEdPublicKey()
    {
        // Act
        var edPublicKey = XEdDSASigning.ConvertX25519PublicKeyToEd25519(_testPublicKey);

        // Assert
        Assert.That(edPublicKey, Is.Not.Null);
        Assert.That(edPublicKey.Length, Is.EqualTo(32));
        Assert.That(edPublicKey[31] & 0x80, Is.EqualTo(0)); // Sign bit should be 0
    }

    [Test]
    public void Sign_Should_ProduceValidSignature()
    {
        // Arrange
        var message = Encoding.UTF8.GetBytes("Test message for signing");
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act
        var signature = XEdDSASigning.Sign(message, edPrivateKey, edPublicKey, useShortHash: true);

        // Assert
        Assert.That(signature, Is.Not.Null);
        Assert.That(signature.Length, Is.EqualTo(64));
        Assert.That(signature, Is.Not.All.EqualTo(0)); // Should not be all zeros
    }

    [Test]
    public void Verify_Should_ReturnTrue_ForValidSignature()
    {
        // Arrange
        var message = Encoding.UTF8.GetBytes("Test message for verification");
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);
        var signature = XEdDSASigning.Sign(message, edPrivateKey, edPublicKey, useShortHash: true);

        // Act
        var isValid = XEdDSASigning.Verify(message, signature, edPublicKey, useShortHash: true);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Verify_Should_ReturnFalse_ForInvalidSignature()
    {
        // Arrange
        var message = Encoding.UTF8.GetBytes("Test message");
        var invalidSignature = new byte[64]; // All zeros
        var (_, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act
        var isValid = XEdDSASigning.Verify(message, invalidSignature, edPublicKey, useShortHash: true);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Verify_Should_ReturnFalse_ForTamperedMessage()
    {
        // Arrange
        var originalMessage = Encoding.UTF8.GetBytes("Original message");
        var tamperedMessage = Encoding.UTF8.GetBytes("Tampered message");
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);
        var signature = XEdDSASigning.Sign(originalMessage, edPrivateKey, edPublicKey, useShortHash: true);

        // Act
        var isValid = XEdDSASigning.Verify(tamperedMessage, signature, edPublicKey, useShortHash: true);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SignAndVerify_Should_Work_WithBothHashTypes(bool useShortHash)
    {
        // Arrange
        var message = Encoding.UTF8.GetBytes("Test message for both hash types");
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act
        var signature = XEdDSASigning.Sign(message, edPrivateKey, edPublicKey, useShortHash);
        var isValid = XEdDSASigning.Verify(message, signature, edPublicKey, useShortHash);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Sign_Should_ThrowException_ForNullMessage()
    {
        // Arrange
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            XEdDSASigning.Sign(null!, edPrivateKey, edPublicKey));
    }

    [Test]
    public void Sign_Should_HandleEmptyMessage()
    {
        // Arrange
        var message = new byte[0];
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act
        var signature = XEdDSASigning.Sign(message, edPrivateKey, edPublicKey);

        // Assert
        Assert.That(signature, Is.Not.Null);
        Assert.That(signature.Length, Is.EqualTo(64));
    }

    [Test]
    public void Sign_Should_ThrowException_ForInvalidKeySize()
    {
        // Arrange
        var message = Encoding.UTF8.GetBytes("Test message");
        var invalidPrivateKey = new byte[16]; // Wrong size
        var (_, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(_testPrivateKey);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            XEdDSASigning.Sign(message, invalidPrivateKey, edPublicKey));
    }
}

[TestFixture]
public class NodeInfoMessageFactoryTests
{
    private DeviceStateContainer _deviceStateContainer = null!;
    private User _testUser = null!;

    [SetUp]
    public void Setup()
    {
        _deviceStateContainer = new DeviceStateContainer();
        _deviceStateContainer.MyNodeInfo = new MyNodeInfo { MyNodeNum = 0x12345678 };
        _deviceStateContainer.LocalConfig = new LocalConfig
        {
            Lora = new Config.Types.LoRaConfig { HopLimit = 3 }
        };

        _testUser = NodeInfoMessageFactory.CreateTestUser(
            longName: "Test Node",
            shortName: "TN",
            id: "!12345678",
            hardwareModel: HardwareModel.TloraV2
        );
    }

    [Test]
    public void CreateNodeInfoMessage_Should_CreateValidPacket()
    {
        // Act
        var packet = NodeInfoMessageFactory.CreateNodeInfoMessage(_deviceStateContainer, _testUser);

        // Assert
        Assert.That(packet, Is.Not.Null);
        Assert.That(packet.From, Is.EqualTo(0x12345678));
        Assert.That(packet.To, Is.EqualTo(0xffffffff)); // Broadcast
        Assert.That(packet.Decoded, Is.Not.Null);
        Assert.That(packet.Decoded.Portnum, Is.EqualTo(PortNum.NodeinfoApp));
        Assert.That(packet.HopLimit, Is.EqualTo(3));
        Assert.That(packet.WantAck, Is.False);
        Assert.That(packet.Priority, Is.EqualTo(MeshPacket.Types.Priority.Background));
    }

    [Test]
    public void CreateNodeInfoMessage_Should_HandleSigningRequest()
    {
        // Act - Request signing (should not throw even if keys aren't available)
        var packet = NodeInfoMessageFactory.CreateNodeInfoMessage(
            _deviceStateContainer, 
            _testUser, 
            signPacket: true
        );

        // Assert
        Assert.That(packet, Is.Not.Null);
        Assert.That(packet.Decoded, Is.Not.Null);
    }

    [Test]
    public void CreateTestUser_Should_CreateValidUser()
    {
        // Act
        var user = NodeInfoMessageFactory.CreateTestUser();

        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.That(user.LongName, Is.EqualTo("Test User"));
        Assert.That(user.ShortName, Is.EqualTo("TU"));
        Assert.That(user.Id, Is.EqualTo("!12345678"));
        Assert.That(user.HwModel, Is.EqualTo(HardwareModel.Unset));
    }

    [Test]
    public void CreateTestUser_Should_SetPublicKey_WhenProvided()
    {
        // Arrange
        var publicKey = new byte[32];
        new Random().NextBytes(publicKey);

        // Act
        var user = NodeInfoMessageFactory.CreateTestUser(publicKey: publicKey);

        // Assert
        Assert.That(user.PublicKey.ToByteArray(), Is.EqualTo(publicKey));
    }

    [Test]
    public void AnalyzePayloadSizes_Should_ProvideAccurateAnalysis()
    {
        // Act
        var analysis = NodeInfoMessageFactory.AnalyzePayloadSizes(_testUser);

        // Assert
        Assert.That(analysis, Is.Not.Null);
        Assert.That(analysis.UserDataSize, Is.GreaterThan(0));
        Assert.That(analysis.BasePacketSize, Is.GreaterThan(0));
        Assert.That(analysis.UnsignedTotalSize, Is.EqualTo(analysis.BasePacketSize + analysis.UserDataSize));
        Assert.That(analysis.SignedSha256TotalSize, Is.EqualTo(analysis.UnsignedTotalSize + 64));
        Assert.That(analysis.SignedSha512TotalSize, Is.EqualTo(analysis.UnsignedTotalSize + 64));
        Assert.That(analysis.SignatureSizeOverhead, Is.EqualTo(64));
        Assert.That(analysis.Sha256HashSize, Is.EqualTo(32));
        Assert.That(analysis.Sha512HashSize, Is.EqualTo(64));
    }

    [Test]
    public void NodeInfoPayloadAnalysis_Should_CalculateOverheadCorrectly()
    {
        // Arrange
        var analysis = NodeInfoMessageFactory.AnalyzePayloadSizes(_testUser);

        // Assert
        Assert.That(analysis.Sha256Overhead, Is.EqualTo(64));
        Assert.That(analysis.Sha512Overhead, Is.EqualTo(64));
        Assert.That(analysis.Sha256OverheadPercentage, Is.GreaterThan(0));
        Assert.That(analysis.Sha512OverheadPercentage, Is.GreaterThan(0));
        Assert.That(analysis.Sha256OverheadPercentage, Is.EqualTo(analysis.Sha512OverheadPercentage));
    }

    [Test]
    public void VerifyNodeInfoSignature_Should_HandleMissingSignature()
    {
        // Arrange
        var packet = NodeInfoMessageFactory.CreateNodeInfoMessage(_deviceStateContainer, _testUser);
        var publicKey = new byte[32];

        // Act
        var isValid = NodeInfoMessageFactory.VerifyNodeInfoSignature(packet, publicKey);

        // Assert
        Assert.That(isValid, Is.False); // Should return false for missing signature
    }

    [Test]
    public void CreateNodeInfoMessage_Should_ThrowException_ForNullInputs()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            NodeInfoMessageFactory.CreateNodeInfoMessage(null!, _testUser));
        
        Assert.Throws<ArgumentNullException>(() => 
            NodeInfoMessageFactory.CreateNodeInfoMessage(_deviceStateContainer, null!));
    }
}
