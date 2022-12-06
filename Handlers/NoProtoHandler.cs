using Meshtastic.Connections;

namespace Meshtastic.Handlers;

public class NoProtoHandler : ICommandHandler<string> 
{
    public static async Task Handle(string port) 
    {
        var serialConection = new SerialConnection(port); 
        await serialConection.Monitor();
    }
}
