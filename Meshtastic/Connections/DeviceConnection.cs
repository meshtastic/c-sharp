using Meshtastic.Data;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Connections;

public abstract class DeviceConnection
{
    protected ILogger Logger { get; set; }
    protected DeviceStateContainer DeviceStateContainer { get; set; } = new DeviceStateContainer();
    protected List<byte> Buffer { get; set; } = new List<byte>();
    protected int PacketLength { get; set; }

    public DeviceConnection(ILogger logger)
    {
        Logger = logger;
    }

    public virtual Task Monitor()
    {
        throw new NotImplementedException();
    }

    public abstract Task WriteToRadio(ToRadio toRadio, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete);

    public abstract Task WriteToRadio(ToRadio toRadio);

    public abstract void Disconnect();

    public abstract Task ReadFromRadio(Func<FromRadio?, DeviceStateContainer, Task<bool>> isComplete,
        int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT);

    protected async Task<bool> ParsePackets(byte item, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete)
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
            if (bufferIndex == PacketFraming.PACKET_HEADER_LENGTH - 1 && PacketLength > Resources.MAX_TO_FROM_RADIO_LENGTH)
            {
                Logger.LogTrace("Packet failed size validation");
                Buffer.Clear();
            }

            if (Buffer.Count > 0 && (bufferIndex + 1) >= (PacketLength + PacketFraming.PACKET_HEADER_LENGTH))
            {
                var payload = Buffer.Skip(PacketFraming.PACKET_HEADER_LENGTH).ToArray();
                var message = new FromDeviceMessage(Logger);
                var fromRadio = message.ParsedFromRadio(payload);

                if (fromRadio != null)
                {
                    DeviceStateContainer.AddFromRadio(fromRadio);
                    Logger.LogDebug($"Received: {fromRadio}");
                    if (await isComplete(fromRadio, DeviceStateContainer))
                    {
                        Buffer.Clear();
                        return true;
                    }
                }
                else
                {
                    Logger.LogDebug($"Notification of pending packets {Convert.ToBase64String(payload)}");
                }
                Buffer.Clear();
            }
        }
        return false;
    }
}