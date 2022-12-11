using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using System.ComponentModel;
using System.Net.Sockets;

namespace Meshtastic.Cli.Handlers;

public class SetCommandHandler : DeviceCommandHandler
{
    public async Task<int> Handle(IEnumerable<string> settings, DeviceConnectionContext context, ILogger logger)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: false);
        if (!isValid)
            return 1;

        var connection = context.GetDeviceConnection();
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();

        await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        return 0;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        return Task.CompletedTask;
    }
}
