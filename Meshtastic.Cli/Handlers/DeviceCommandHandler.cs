using Meshtastic.Cli.Parsers;
using Meshtastic.Connections;
using Meshtastic.Data;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Handlers;

public class DeviceCommandHandler
{
    protected DeviceConnection? connection;
    protected ToRadioMessageFactory ToRadioMessageFactory = new();

    protected async Task OnConnection(DeviceConnectionContext context, Func<Task> operation)
    {
        await AnsiConsole.Status()
            .StartAsync("Connecting...", async ctx =>
            {
                ctx.Status($"Connecting {context.DisplayName}...");
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(new Style(StyleResources.MESHTASTIC_GREEN));

                await operation();
            });
    }

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

    public static Task<bool> AdminMessageResponseReceived(FromDeviceMessage packet, DeviceStateContainer container)
    {
        if (packet.ParsedMessage.adminMessage != null)
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public static async Task<bool> AlwaysComplete(FromDeviceMessage packet, DeviceStateContainer container)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> DefaultIsCompleteAsync(FromDeviceMessage packet, DeviceStateContainer container)
    {
        if (packet.ParsedMessage.fromRadio?.PayloadVariantCase != FromRadio.PayloadVariantOneofCase.ConfigCompleteId)
            return false;

        await OnCompleted(packet, container);
        return true;
    }

    public virtual async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        await Task.CompletedTask;
    }
}