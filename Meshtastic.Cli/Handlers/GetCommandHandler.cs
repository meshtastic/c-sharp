using Meshtastic.Connections;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;

namespace Meshtastic.Cli.Handlers;

public class GetCommandHandler : DeviceCommandHandler
{
    private IEnumerable<ParsedSetting>? parsedSettings;
    public async Task Handle(IEnumerable<string> settings, DeviceConnectionContext context, ILogger logger)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: true);
        if (!isValid)
            return;

        parsedSettings = result!.ParsedSettings;

        await OnConnection(context, async () =>
        {
            var connection = context.GetDeviceConnection();
            var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container);
        printer.PrintSettings(parsedSettings!);
        return Task.CompletedTask;
    }
}
