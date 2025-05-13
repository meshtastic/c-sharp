namespace Meshtastic.Crypto;
public class NonceGenerator
{
    private readonly byte[] nonce = new byte[16];

    public NonceGenerator(uint fromNum, ulong packetId)
    {
        InitNonce(fromNum, packetId);
    }

    public byte[] Create() => nonce;

    private void InitNonce(uint fromNode, ulong packetId)
    {
        Array.Clear(nonce, 0, nonce.Length);

        Buffer.BlockCopy(BitConverter.GetBytes(packetId), 0, nonce, 0, sizeof(ulong));
        Buffer.BlockCopy(BitConverter.GetBytes(fromNode), 0, nonce, sizeof(ulong), sizeof(uint));
    }
}