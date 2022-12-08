using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Connections;

public interface IDeviceConnection
{
    DeviceStateContainer DeviceStateContainer { get; set; }

    Task WriteToRadio(byte[] data);
    Task ReadFromRadio(Func<FromRadio, bool> isComplete, int readTimeoutMs = Resources.DEFAULT_READ_TIMEOUT);
}