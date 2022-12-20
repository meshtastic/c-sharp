using Meshtastic.Cli.Binders;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class MonitorCommand : Command
{
    public MonitorCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var monitorCommandHandler = new MonitorCommandHandler();
        this.SetHandler(monitorCommandHandler.Handle,
            new ConnectionBinder(port, host),
            new LoggingBinder());
    }
}

public class MonitorCommandHandler
{ 
    public async Task Handle(DeviceConnectionContext context, ILogger logger) 
    {
        var connection = context.GetDeviceConnection();
        await connection.Monitor();
    }
}
