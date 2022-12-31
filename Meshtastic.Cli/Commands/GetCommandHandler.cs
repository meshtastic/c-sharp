using Meshtastic.Cli.Parsers;
using Meshtastic.Data;
using Meshtastic.Display;

namespace Meshtastic.Cli.Commands;

public class GetCommandHandler : DeviceCommandHandler
{
    private readonly IEnumerable<ParsedSetting>? parsedSettings;

    public GetCommandHandler(IEnumerable<string> settings,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: true);
        if (!isValid)
            return;

        parsedSettings = result!.ParsedSettings;
    }

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container, OutputFormat);
        printer.PrintSettings(parsedSettings!);
        return Task.CompletedTask;
    }
}
