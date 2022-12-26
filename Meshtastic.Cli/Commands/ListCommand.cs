using Meshtastic.Cli.Enums;
using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class ListCommand : Command
{
    public ListCommand(string name, string description, Option<OutputFormat> _, Option<LogLevel> __) 
        : base(name, description)
    { 
        var listCommandHandler = new ListCommandHandler();
        this.SetHandler(ListCommandHandler.Handle);
    }
}
public class ListCommandHandler
{ 
    public static async Task Handle() 
    {
        AnsiConsole.WriteLine("Found the following serial ports:");
        foreach (var port in SerialConnection.ListPorts())
            AnsiConsole.WriteLine(port);
        await Task.CompletedTask;
    }
}
