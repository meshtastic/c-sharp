using Meshtastic.Data;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Connections;

public class SimulatedConnection : DeviceConnection
{
    public SimulatedConnection(ILogger logger) : base(logger)
    {
        Logger = logger;
    }

    public new ILogger Logger { get; }

    public override Task ReadFromRadio(Func<FromRadio?, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = 15000)
    {
        throw new NotImplementedException();
    }

    public override Task WriteToRadio(ToRadio toRadio, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete)
    {
        throw new NotImplementedException();
    }
}