using Meshtastic.Data;

namespace Meshtastic.Connections;

public abstract class DeviceConnection
{
    protected DeviceStateContainer DeviceStateContainer { get; set; } = new DeviceStateContainer();
    protected List<byte> Buffer { get; set; } = new List<byte>();
    protected int PacketLength { get; set; }
    public abstract Task Monitor();
    public abstract Task WriteToRadio(byte[] data, Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete);
    public abstract Task ReadFromRadio(Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete, 
        int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT);

    protected async Task<bool> ParsePackets(byte item, Func<FromDeviceMessage, DeviceStateContainer, Task<bool>> isComplete) 
    {
        int bufferIndex = Buffer.Count;
        Buffer.Add(item);
        if (bufferIndex == 0 && item != PacketFraming.PACKET_FRAME_START[0]) 
            Buffer.Clear();
        else if (bufferIndex == 1 && item != PacketFraming.PACKET_FRAME_START[1])
            Buffer.Clear();
        else if (bufferIndex >= PacketFraming.PACKET_HEADER_LENGTH - 1) 
        {
            PacketLength = (Buffer[2] << 8) + Buffer[3];
            // Packet fails size validation
            if (bufferIndex == PacketFraming.PACKET_HEADER_LENGTH - 1 && PacketLength > Resources.MAX_TO_FROM_RADIO_LENGTH) 
                Buffer.Clear();

            if (Buffer.Count > 0 && (bufferIndex + 1) >= (PacketLength + PacketFraming.PACKET_HEADER_LENGTH))
            {
                try 
                {
                    var payload = Buffer.Skip(PacketFraming.PACKET_HEADER_LENGTH).ToArray();
                    var message = new FromDeviceMessage(payload);

                    if (message.ParsedMessage.fromRadio != null)
                        DeviceStateContainer.AddFromRadio(message.ParsedMessage.fromRadio!);
                    //else
                    //    Console.WriteLine(payload);

                    if (await isComplete(message, DeviceStateContainer))
                    {
                        Buffer.Clear();
                        return true;
                    }
                }
                catch (Exception ex) 
                {
                    //Console.WriteLine(ex);
                }
                Buffer.Clear();
            }
        }
        return false;
    }
}