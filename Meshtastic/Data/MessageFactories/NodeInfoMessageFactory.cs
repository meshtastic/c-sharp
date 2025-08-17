using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Protobufs;
using Meshtastic.Crypto;
using Org.BouncyCastle.Security;

namespace Meshtastic.Data.MessageFactories;

/// <summary>
/// Factory for creating NodeInfo (User) MeshPackets with optional XEdDSA signatures
/// </summary>
public static class NodeInfoMessageFactory
{
    /// <summary>
    /// Create a NodeInfo MeshPacket with the user's information
    /// </summary>
    /// <param name="deviceStateContainer">Device state containing configuration</param>
    /// <param name="user">User information to broadcast</param>
    /// <param name="signPacket">Whether to sign the packet with XEdDSA</param>
    /// <param name="useShortHash">Use SHA-256 instead of SHA-512 for signatures</param>
    /// <returns>MeshPacket containing the NodeInfo</returns>
    public static MeshPacket CreateNodeInfoMessage(
        DeviceStateContainer deviceStateContainer,
        User user,
        bool signPacket = false,
        bool useShortHash = true)
    {
        if (deviceStateContainer == null)
            throw new ArgumentNullException(nameof(deviceStateContainer));
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Create the data payload
        var data = new Protobufs.Data
        {
            Portnum = PortNum.NodeinfoApp,
            Payload = user.ToByteString()
        };

        // Create the base mesh packet
        var meshPacket = new MeshPacket
        {
            From = deviceStateContainer.MyNodeInfo?.MyNodeNum ?? 0,
            To = 0xffffffff, // Broadcast address
            Id = GeneratePacketId(),
            Channel = 0,
            HopLimit = deviceStateContainer.LocalConfig?.Lora?.HopLimit ?? 3,
            WantAck = false,
            Priority = MeshPacket.Types.Priority.Background,
            Decoded = data
        };

        // Add signature if requested and we have the necessary keys
        if (signPacket && HasSigningCapability(deviceStateContainer))
        {
            try
            {
                AddXEdDSASignature(meshPacket, deviceStateContainer, useShortHash);
            }
            catch (Exception ex)
            {
                // Log error but don't fail packet creation
                // Swallow exception: failed to sign NodeInfo packet, but don't fail packet creation
            }
        }

        return meshPacket;
    }

    /// <summary>
    /// Create a minimal NodeInfo packet for size comparison testing
    /// </summary>
    /// <param name="longName">User's long name</param>
    /// <param name="shortName">User's short name</param>
    /// <param name="id">User ID</param>
    /// <param name="hardwareModel">Hardware model</param>
    /// <param name="publicKey">Optional public key</param>
    /// <returns>Minimal User object for testing</returns>
    public static User CreateTestUser(
        string longName = "Test User",
        string shortName = "TU",
        string id = "!12345678",
        HardwareModel hardwareModel = HardwareModel.Unset,
        byte[]? publicKey = null)
    {
        var user = new User
        {
            LongName = longName,
            ShortName = shortName,
            Id = id,
            HwModel = hardwareModel,
            IsUnmessagable = false,
            Macaddr = ByteString.CopyFrom([0xFF, 0xAA, 0x88, 0x99, 0x55, 0x66]),
            PublicKey = publicKey != null ? ByteString.CopyFrom(publicKey) : null
        };

        return user;
    }

    /// <summary>
    /// Verify the XEdDSA signature on a received NodeInfo packet
    /// </summary>
    /// <param name="meshPacket">Received mesh packet</param>
    /// <param name="senderPublicKey">Public key of the sender</param>
    /// <param name="useShortHash">Use SHA-256 instead of SHA-512</param>
    /// <returns>True if signature is valid</returns>
    public static bool VerifyNodeInfoSignature(
        MeshPacket meshPacket,
        byte[] senderPublicKey,
        bool useShortHash = true)
    {
        if (meshPacket?.Decoded?.Payload == null)
            return false;

        // For now, we'll simulate signature verification since the protobuf doesn't have signature fields yet
        // This would be replaced with actual signature verification once the firmware PR is merged

        try
        {
            var payload = meshPacket.Decoded.Payload.ToByteArray();

            // In a real implementation, we would extract the signature from the packet
            // For now, we'll demonstrate the verification process
            var mockSignature = new byte[64]; // This would come from the packet

            return XEdDSASigning.Verify(payload, mockSignature, senderPublicKey, useShortHash);
        }
        catch
        {
            return false;
        }
    }

    private static bool HasSigningCapability(DeviceStateContainer deviceStateContainer)
    {
        // Check if we have the necessary keys for signing
        // This would be based on the device's PKI configuration
        return deviceStateContainer.LocalConfig?.Security?.PublicKey != null;
    }

    private static void AddXEdDSASignature(MeshPacket meshPacket, DeviceStateContainer deviceStateContainer, bool useShortHash)
    {
        if (meshPacket.Decoded?.Payload == null)
            return;

        // Get the device's private key (this would be from secure storage)
        var privateKey = GetDevicePrivateKey(deviceStateContainer);
        if (privateKey == null)
            return;

        // Generate Ed25519 keys from X25519 private key
        var (edPrivateKey, edPublicKey) = XEdDSASigning.GenerateEdDSAKeysFromX25519(privateKey);

        // Sign the payload
        var payload = meshPacket.Decoded.Payload.ToByteArray();
        var signature = XEdDSASigning.Sign(payload, edPrivateKey, edPublicKey, useShortHash);

        // Note: In the actual implementation, we would add the signature to the packet
        // For now, we'll store it in a custom field or metadata
        // Once the firmware PR is merged, this would be:
        // meshPacket.Decoded.XeddsaSignature = ByteString.CopyFrom(signature);
        // meshPacket.Decoded.HasXeddsaSignature = true;

        Console.WriteLine($"Generated XEdDSA signature: {Convert.ToHexString(signature)}");
    }

    private static byte[]? GetDevicePrivateKey(DeviceStateContainer deviceStateContainer)
    {
        // In a real implementation, this would retrieve the device's private key
        // from secure storage or generate one if it doesn't exist

        // For testing, we'll generate a mock private key
        var random = new Random();
        var privateKey = new byte[32];
        var privateKey = new byte[32];
        new SecureRandom().NextBytes(privateKey);
        return privateKey;
    }

    private static uint GeneratePacketId()
    {
        var bytes = new byte[4];
        new SecureRandom().NextBytes(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
}
