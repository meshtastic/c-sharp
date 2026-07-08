using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using System.Text;
using Meshtastic.Protobufs;

namespace Meshtastic.Crypto;

/// <summary>
/// XEdDSA (Extended EdDSA) implementation for signing NodeInfo packets
/// Simplified implementation for demonstration and payload size analysis
/// </summary>
public static class XEdDSASigning
{
    /// <summary>
    /// Generate an Ed25519 key pair for signing (simplified approach)
    /// </summary>
    /// <returns>Tuple of (Ed25519 private key, Ed25519 public key)</returns>
    public static (byte[] edPrivateKey, byte[] edPublicKey) GenerateSigningKeyPair()
    {
        var keyPairGen = new Ed25519KeyPairGenerator();
        keyPairGen.Init(new KeyGenerationParameters(new SecureRandom(), 256));
        
        var keyPair = keyPairGen.GenerateKeyPair();
        var privateKey = ((Ed25519PrivateKeyParameters)keyPair.Private).GetEncoded();
        var publicKey = ((Ed25519PublicKeyParameters)keyPair.Public).GetEncoded();
        
        return (privateKey, publicKey);
    }

    /// <summary>
    /// Generate Ed25519 keys from an X25519 private key (simplified for demo)
    /// </summary>
    /// <param name="x25519PrivateKey">32-byte X25519 private key</param>
    /// <returns>Tuple of (Ed25519 private key, Ed25519 public key)</returns>
    public static (byte[] edPrivateKey, byte[] edPublicKey) GenerateEdDSAKeysFromX25519(byte[] x25519PrivateKey)
    {
        if (x25519PrivateKey.Length != 32)
            throw new ArgumentException("X25519 private key must be 32 bytes", nameof(x25519PrivateKey));

        var ed25519PrivateKey = new byte[32];
        Array.Copy(x25519PrivateKey, ed25519PrivateKey, ed25519PrivateKey.Length);
        // Clamp X25519
        ed25519PrivateKey[0] &= 0xF8;
        ed25519PrivateKey[31] &= 0x7F;
        ed25519PrivateKey[31] |= 0x40;

        var x25519PublicKey = PKIEncryption.GetPublicKeyFromPrivateKey(x25519PrivateKey);
        var ed25519PublicKey = ConvertX25519PublicKeyToEd25519(x25519PublicKey, forcePositive: false);

        // If resulting public key is positive, return as-is
        if ((ed25519PublicKey[31] & 0x80) == 0) return (x25519PrivateKey, ed25519PublicKey);

        // Ed25519 Group Order
        var L = new BigInteger("7237005577332262213973186563042994240857116359379907606001950938285454250989");

        // Negate private key
        var privateKeyScalar = new BigInteger(ed25519PrivateKey.Reverse().ToArray());
        var negatedPrivateKeyScalar = L.Subtract(privateKeyScalar);
        var negatedPrivateKeyBytes = negatedPrivateKeyScalar.ToByteArrayUnsigned();
        byte[] negatedPrivateKey = new byte[32];
        Array.Copy(negatedPrivateKeyBytes, 0, negatedPrivateKey, 0, negatedPrivateKeyBytes.Length);

        // Recompute public key from negated privated key
        var negatedPublicKey = ConvertX25519PublicKeyToEd25519(negatedPrivateKey);

        return (negatedPrivateKey, negatedPublicKey);
    }

    /// <summary>
    /// Convert X25519 public key to Ed25519 public key (simplified for demo)
    /// </summary>
    /// <param name="x25519PublicKey">32-byte X25519 public key</param>
    /// <returns>32-byte Ed25519 public key</returns>
    public static byte[] ConvertX25519PublicKeyToEd25519(byte[] x25519PublicKey, bool forcePositive = false)
    {
        if (x25519PublicKey.Length != 32)
            throw new ArgumentException("X25519 public key must be 32 bytes", nameof(x25519PublicKey));

        // Clear the sign bit (bit 7 of the last byte) as per Ed25519 specification
        // Implements the birational map from Montgomery (X25519) u to Edwards (Ed25519) y:
        // y = (u - 1) / (u + 1) mod p
        // See: https://tools.ietf.org/html/rfc7748#section-5

        // Curve25519 prime: 2^255 - 19
        // Ed25519 field prime: 2^255 - 19
        BigInteger p = BigInteger.ValueOf(2).Pow(255).Subtract(BigInteger.ValueOf(19));

        // Interpret the X25519 public key as a little-endian integer u
        byte[] uBytes = new byte[32];
        Array.Copy(x25519PublicKey, uBytes, 32);
        // Ensure top bit is masked (Montgomery u-coordinate is 255 bits)
        uBytes[31] &= 0x7F;
        BigInteger u = new BigInteger(1, uBytes.Reverse().ToArray());

        // Compute y = (u - 1) * (u + 1)^-1 mod p
        BigInteger one = BigInteger.One;
        BigInteger uMinus1 = u.Subtract(one).Mod(p);
        BigInteger uPlus1 = u.Add(one).Mod(p);
        BigInteger uPlus1Inv = uPlus1.ModInverse(p);
        BigInteger y = uMinus1.Multiply(uPlus1Inv).Mod(p);

        // Encode y as 32-byte little-endian
        byte[] yBytes = y.ToByteArrayUnsigned();
        byte[] edPublicKeyResult = new byte[32];
        // Copy yBytes into edPublicKey (little-endian)
        for (int i = 0; i < yBytes.Length && i < 32; i++)
        {
            edPublicKeyResult[i] = yBytes[yBytes.Length - 1 - i];
        }
        // If yBytes is shorter than 32 bytes, the rest is already zero

        // Set the sign bit to 0 (positive x)
        if (forcePositive) edPublicKeyResult[31] &= 0x7F;
        return edPublicKeyResult;
    }

    private static byte[] BuildSigningBuffer(MeshPacket meshPacket)
    {
        return [
            ..BitConverter.GetBytes(meshPacket.From),
            ..BitConverter.GetBytes(meshPacket.Id),
            ..BitConverter.GetBytes((uint)meshPacket.Decoded.Portnum),
            ..meshPacket.Decoded.Payload.ToByteArray()
        ];
    }

    /// <summary>
    /// Verify MeshPacket signature using provided node's X25519 public key.
    /// </summary>
    /// <param name="senderPublicKey">Public key of sender node.</param>
    /// <param name="meshPacket">Packet to verify.</param>
    /// <returns>True is packet signature is valid</returns>
    public static bool VerifyPacketSignature(byte[] senderPublicKey, MeshPacket meshPacket)
    {
        var message = BuildSigningBuffer(meshPacket);
        var signature = meshPacket.Decoded.XeddsaSignature.ToByteArray();
        var edPublicKey = ConvertX25519PublicKeyToEd25519(senderPublicKey);
        return Verify(message, signature, edPublicKey);
    }

    /// <summary>
    /// Adds a signature to provided MeshPacket.
    /// </summary>
    /// <param name="senderPrivateKey">Private X25519 key of packet sender.</param>
    /// <param name="meshPacket">Packet to sign.</param>
    public static void AddPacketSignature(byte[] senderPrivateKey, MeshPacket meshPacket)
    {
        var message = BuildSigningBuffer(meshPacket);
        var (edPrivateKey, edPublicKey) = GenerateEdDSAKeysFromX25519(senderPrivateKey);
        var signature = Sign(message, edPrivateKey, edPublicKey);
        meshPacket.Decoded.XeddsaSignature = Google.Protobuf.ByteString.CopyFrom(signature);
    }

    /// <summary>
    /// Sign a message using Ed25519
    /// </summary>
    /// <param name="message">Message to sign</param>
    /// <param name="edPrivateKey">Ed25519 private key</param>
    /// <param name="useShortHash">Use SHA-256 instead of SHA-512 for hashing before signing</param>
    /// <returns>64-byte signature</returns>
    public static byte[] Sign(byte[] message, byte[] edPrivateKey, byte[] edPublicKey)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (edPrivateKey.Length != 32) throw new ArgumentException("Ed25519 private key must be 32 bytes", nameof(edPrivateKey));

        // Create Ed25519 signer
        var signer = new Ed25519Signer();
        var privateKeyParams = new Ed25519PrivateKeyParameters(edPrivateKey, 0);
        
        signer.Init(true, privateKeyParams);
        signer.BlockUpdate(message, 0, message.Length);
        
        return signer.GenerateSignature();
    }

    /// <summary>
    /// Verify an Ed25519 signature
    /// </summary>
    /// <param name="message">Original message</param>
    /// <param name="signature">64-byte signature</param>
    /// <param name="edPublicKey">Ed25519 public key of the signer</param>
    /// <param name="useShortHash">Use SHA-256 instead of SHA-512 for hashing</param>
    /// <returns>True if signature is valid</returns>
    public static bool Verify(byte[] message, byte[] signature, byte[] edPublicKey)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (signature == null || signature.Length != 64) throw new ArgumentException("Signature must be 64 bytes", nameof(signature));
        if (edPublicKey == null || edPublicKey.Length != 32) throw new ArgumentException("Ed25519 public key must be 32 bytes", nameof(edPublicKey));

        try
        {
            // Create Ed25519 verifier
            var verifier = new Ed25519Signer();
            var publicKeyParams = new Ed25519PublicKeyParameters(edPublicKey, 0);
            
            verifier.Init(false, publicKeyParams);
            verifier.BlockUpdate(message, 0, message.Length);
            
            return verifier.VerifySignature(signature);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verify a signature using X25519 public key (for compatibility)
    /// </summary>
    /// <param name="message">Original message</param>
    /// <param name="signature">64-byte signature</param>
    /// <param name="x25519PublicKey">X25519 public key of the signer</param>
    /// <param name="useShortHash">Use SHA-256 instead of SHA-512 for verification</param>
    /// <returns>True if signature is valid</returns>
    public static bool VerifyWithX25519Key(byte[] message, byte[] signature, byte[] x25519PublicKey)
    {
        try
        {
            // Convert X25519 public key to Ed25519 public key
            var edPublicKey = ConvertX25519PublicKeyToEd25519(x25519PublicKey);
            return Verify(message, signature, edPublicKey);
        }
        catch
        {
            return false;
        }
    }
}
