using Meshtastic.Connections;

namespace Meshtastic.Cli.CommandHandlers;

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
