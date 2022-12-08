using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Handlers;

public class InfoCommandHandler : ICommandHandler<string> 
{
    public static async Task Handle(string port, ILogger logger)
    {
        var serialConection = new SerialConnection(port);
        var wantConfig = new ToRadio();
        wantConfig.WantConfigId = (uint)Random.Shared.Next();
        await serialConection.WriteToRadio(wantConfig.ToByteArray());
    }
}
