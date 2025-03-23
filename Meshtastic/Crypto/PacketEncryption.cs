using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Meshtastic.Crypto;

public static class PacketEncryption
{
    private static readonly IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

    public static byte[] TransformPacket(byte[] cypherText, byte[] nonce, byte[] key)
    {
        cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), nonce));
        byte[] output = new byte[cipher.GetOutputSize(cypherText.Length)];
        _ = cipher.DoFinal(cypherText, output);

        return output;
    }
}
