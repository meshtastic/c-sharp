using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Handlers;

public class ListCommandHandler
{ 
    public async Task Handle(ILogger logger) 
    {
        AnsiConsole.WriteLine("Found the following serial ports:");
        foreach (var port in SerialConnection.ListPorts())
            AnsiConsole.WriteLine(port);
    }
}
