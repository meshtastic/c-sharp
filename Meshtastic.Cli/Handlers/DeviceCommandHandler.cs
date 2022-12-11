using Meshtastic.Cli.Parsers;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Handlers;

public class DeviceCommandHandler
{
    protected static (SettingParserResult? result, bool isValid) ParseSettingOptions(IEnumerable<string> settings, bool isGetOnly)
    {
        var parser = new SettingParser(settings);
        var parserResult = parser.ParseSettings(isGetOnly);

        if (parserResult.ValidationIssues.Any())
        {
            foreach (var issue in parserResult.ValidationIssues)
            {
                AnsiConsole.Markup($":warning: [red]{issue}[/]");
            }
            return (null, false);
        }

        return (parserResult, true);
    }

    public async Task<bool> DefaultIsCompleteAsync(FromRadio packet, DeviceStateContainer container)
    {
        if (packet.PayloadVariantCase != FromRadio.PayloadVariantOneofCase.ConfigCompleteId)
            return false;

        await OnCompleted(packet, container);
        return true;
    }

    public virtual async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        await Task.CompletedTask;
    }
}