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

    public override async Task ReadFromRadio(Func<FromRadio?, DeviceStateContainer, Task<bool>> isComplete, int readTimeoutMs = 15000)
    {
        await Task.CompletedTask;
    }

    public override async Task WriteToRadio(ToRadio toRadio, Func<FromRadio, DeviceStateContainer, Task<bool>> isComplete)
    {
        await Task.CompletedTask;
    }
}