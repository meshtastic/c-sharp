using Meshtastic.Cli.Parsers;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.CommandHandlers;

public class GetCommandHandler : DeviceCommandHandler
{
    public readonly IEnumerable<ParsedSetting>? ParsedSettings;

    public GetCommandHandler(IEnumerable<string> settings,
        DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: true);
        if (!isValid)
            return;

        ParsedSettings = result!.ParsedSettings;
    }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container, OutputFormat);
        printer.PrintSettings(ParsedSettings!);
        return Task.CompletedTask;
    }
}
