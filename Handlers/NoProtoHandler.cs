using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Handlers;

public class NoProtoHandler : ICommandHandler<string> 
{
    public static async Task Handle(string port, ILogger logger) 
    {
        var serialConection = new SerialConnection(port); 
        await serialConection.Monitor();
    }
}
