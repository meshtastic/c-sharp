using System.Security.Cryptography;
using Google.Protobuf;
using Meshtastic.Protobufs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Meshtastic.Crypto;

public static class PKIEncryption
{
	public static (byte[] privateKey, byte[] publicKey) GenerateKeyPair()
	{
		var privateKey = GeneratePrivateKey();
		return (privateKey, GetPublicKeyFromPrivateKey(privateKey));
	}

	public static byte[] GeneratePrivateKey() => new X25519PrivateKeyParameters(new SecureRandom()).GetEncoded();
	public static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey) => LoadPrivateKey(privateKey).GeneratePublicKey().GetEncoded();

	private static X25519PrivateKeyParameters LoadPrivateKey(byte[] privateKey) => new X25519PrivateKeyParameters(privateKey, 0);
	private static X25519PublicKeyParameters LoadPublicKey(byte[] publicKey) => new X25519PublicKeyParameters(publicKey, 0);

	private static byte[] GenerateNonce(uint packetId, uint senderNodeId, uint extraNonce)
		=> [
			..BitConverter.GetBytes(packetId),
			..BitConverter.GetBytes(extraNonce),
			..BitConverter.GetBytes(senderNodeId),
			0
		];

	public static bool Decrypt(byte[] recipientPrivateKey, byte[] senderPublicKey, MeshPacket meshPacket)
	{
		if (meshPacket.Encrypted == null || meshPacket.Encrypted.Length == 0) return false;
		var encryptedData = meshPacket.Encrypted.ToByteArray();
		var decrypted = Decrypt(recipientPrivateKey, senderPublicKey, encryptedData, meshPacket.Id, meshPacket.From);
		var data = Protobufs.Data.Parser.ParseFrom(decrypted);
		meshPacket.Decoded = data;
		return true;
	}

	public static byte[] Decrypt(byte[] recipientPrivateKey, byte[] senderPublicKey, byte[] encryptedData, uint packetId, uint senderNodeId)
		=> Decrypt(LoadPrivateKey(recipientPrivateKey), LoadPublicKey(senderPublicKey), encryptedData, packetId, senderNodeId);

	private static byte[] GenerateSharedKey(X25519PrivateKeyParameters privateKey, X25519PublicKeyParameters publicKey)
	{
		var sharedKey = new byte[32];
		privateKey.GenerateSecret(publicKey, sharedKey);
		var hashedSharedKey = SHA256.HashData(sharedKey);
		return hashedSharedKey;
	}

	private static byte[] Decrypt(X25519PrivateKeyParameters recipientPrivateKey, X25519PublicKeyParameters senderPublicKey, byte[] encryptedData, uint packetId, uint senderNodeId)
	{
		var sharedKey = GenerateSharedKey(recipientPrivateKey, senderPublicKey);
		var extraNonce = BitConverter.ToUInt32(encryptedData, encryptedData.Length - 4);
		var nonce = GenerateNonce(packetId, senderNodeId, extraNonce);

		var cipher = new CcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
		var parameters = new AeadParameters(new KeyParameter(sharedKey), 64, nonce);
		cipher.Init(false, parameters);

		byte[] output = new byte[cipher.GetOutputSize(encryptedData.Length - 4)];
		int len = cipher.ProcessBytes(encryptedData, 0, encryptedData.Length - 4, output, 0);
		cipher.DoFinal(output, len);
		return output;
	}

	public static bool Encrypt(byte[] senderPrivateKey, byte[] recipientPublicKey, MeshPacket meshPacket)
	{
		if (meshPacket.Encrypted != null && meshPacket.Encrypted.Length > 0) return false;
		if (meshPacket.Decoded == null) return false;
		var plaintext = meshPacket.Decoded.ToByteArray();
		var encrypted = Encrypt(senderPrivateKey, recipientPublicKey, plaintext, meshPacket.Id, meshPacket.From);
		meshPacket.Encrypted = ByteString.CopyFrom(encrypted);
		return true;
	}

	public static byte[] Encrypt(byte[] senderPrivateKey, byte[] recipientPublicKey, byte[] plaintext, uint packetId, uint senderNodeId)
		=> Encrypt(LoadPrivateKey(senderPrivateKey), LoadPublicKey(recipientPublicKey), plaintext, packetId, senderNodeId);
	
	private static byte[] Encrypt(X25519PrivateKeyParameters senderPrivateKey, X25519PublicKeyParameters recipientPublicKey, byte[] plaintext, uint packetId, uint senderNodeId)
	{
		var extraNonceBytes = new byte[4];
		new SecureRandom().NextBytes(extraNonceBytes);
		var extraNonce = BitConverter.ToUInt32(extraNonceBytes, 0);
		var sharedKey = GenerateSharedKey(senderPrivateKey, recipientPublicKey);
		var nonce = GenerateNonce(packetId, senderNodeId, extraNonce);

		var cipher = new CcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
		var parameters = new AeadParameters(new KeyParameter(sharedKey), 64, nonce);

		cipher.Init(true, parameters);

		byte[] output = new byte[cipher.GetOutputSize(plaintext.Length) + 4];
		int len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, output, 0);
		cipher.DoFinal(output, len);
		var extraNonceBits = BitConverter.GetBytes(extraNonce);
		Array.Copy(extraNonceBits, 0, output, output.Length - 4, 4);
		return output;
	}
}
