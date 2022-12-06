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
        wantConfig.WantConfigId = 1;

        using (var memoryStream = new MemoryStream()) 
        {
            using (var codedOutputStream = new CodedOutputStream(memoryStream))
            wantConfig.WriteTo(codedOutputStream);
            await serialConection.WriteToRadio(memoryStream.ToArray());
            await Task.Delay(5000);
        }
    }
}
