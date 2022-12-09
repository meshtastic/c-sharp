using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Handlers;

public class InfoCommandHandler : ICommandHandler<DeviceConnectionContext> 
{
    public static async Task Handle(DeviceConnectionContext context, ILogger logger)
    {
        var connection = context.GetDeviceConnection();
        var wantConfig = new ToRadio
        {
            WantConfigId = (uint)Random.Shared.Next()
        };
        await connection.WriteToRadio(wantConfig.ToByteArray(),
            (packet, deviceStateContainer) => {
                if (packet.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.ConfigCompleteId) 
                {
                    ProtobufPrinter.Print(deviceStateContainer);
                    return true;
                }
                return false;
            });
    }
}
