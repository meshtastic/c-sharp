using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Google.Protobuf;

namespace Meshtastic.Handlers;

public class InfoCommandHandler : ICommandHandler<string> 
{
    public static async Task Handle(string port)
    {
        var serialConection = new SerialConnection(port);
        var wantConfig = new ToRadio();
        wantConfig.WantConfigId = (uint)Random.Shared.Next();

        Console.WriteLine("Sending:");
        Console.WriteLine(wantConfig.ToString());

        await serialConection.WriteToRadio(wantConfig.ToByteArray());
    }
}
