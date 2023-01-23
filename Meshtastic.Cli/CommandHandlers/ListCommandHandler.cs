using Meshtastic.Connections;
using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Cli.CommandHandlers;

[ExcludeFromCodeCoverage(Justification = "Requires serial hardware")]
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
