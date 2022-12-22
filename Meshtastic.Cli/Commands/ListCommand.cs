using Meshtastic.Cli.Binders;
using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class ListCommand : Command
{
    public ListCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    { 
        var listCommandHandler = new ListCommandHandler();
        this.SetHandler(listCommandHandler.Handle, new LoggingBinder());
    }
}
public class ListCommandHandler
{ 
    public async Task Handle(ILogger logger) 
    {
        AnsiConsole.WriteLine("Found the following serial ports:");
        foreach (var port in SerialConnection.ListPorts())
            AnsiConsole.WriteLine(port);
        await Task.CompletedTask;
    }
}
