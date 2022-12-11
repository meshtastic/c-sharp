using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Handlers;

public class MonitorCommandHandler
{ 
    public async Task Handle(DeviceConnectionContext context, ILogger logger) 
    {
        var connection = context.GetDeviceConnection();
        await connection.Monitor();
    }
}
