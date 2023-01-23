using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Parsers;
using Meshtastic.Connections;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class DeviceCommandHandler
{
    protected readonly DeviceConnectionContext ConnectionContext;
    protected readonly DeviceConnection Connection;
    protected readonly ToRadioMessageFactory ToRadioMessageFactory;
    protected readonly OutputFormat OutputFormat;
    protected readonly ILogger Logger;
    protected uint? Destination;
    protected readonly bool SelectDestination;

    public DeviceCommandHandler(DeviceConnectionContext connectionContext, CommandContext commandContext)
    {
        ConnectionContext = connectionContext;
        Connection = connectionContext.GetDeviceConnection(commandContext.Logger);
        ToRadioMessageFactory = new();
        OutputFormat = commandContext.OutputFormat;
        Logger = commandContext.Logger;
        Destination = commandContext.Destination;
        SelectDestination = commandContext.SelectDestination;
    }

    protected static (SettingParserResult? result, bool isValid) ParseSettingOptions(IEnumerable<string> settings, bool isGetOnly)
    {
        var parser = new SettingParser(settings);
        var parserResult = parser.ParseSettings(isGetOnly);

        if (parserResult.ValidationIssues.Any())
        {
            foreach (var issue in parserResult.ValidationIssues)
            {
                AnsiConsole.MarkupLine($"[red]{issue}[/]");
            }
            return (null, false);
        }

        return (parserResult, true);
    }

    public static Task<bool> AdminMessageResponseReceived(FromRadio fromRadio, DeviceStateContainer container)
    {
        if (fromRadio.GetMessage<AdminMessage>() != null)
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public static async Task<bool> AnyResponseReceived(FromRadio fromRadio, DeviceStateContainer container)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> CompleteOnConfigReceived(FromRadio fromRadio, DeviceStateContainer container)
    {
        if (fromRadio.PayloadVariantCase != FromRadio.PayloadVariantOneofCase.ConfigCompleteId)
            return false;

        if (SelectDestination)
        {
            var selection = new SelectionPrompt<uint>()
                    .Title("Please select a destination node")
                    .AddChoices(container.Nodes.Select(n => n.Num));
            selection.Converter = container.GetNodeDisplayName;
            Destination = AnsiConsole.Prompt(selection);
        }

        await OnCompleted(fromRadio, container);
        return true;
    }

    public virtual async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        await Task.CompletedTask;
    }

    protected async Task BeginEditSettings(AdminMessageFactory adminMessageFactory)
    {
        var message = adminMessageFactory.CreateBeginEditSettingsMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message),
            AnyResponseReceived);
        Logger.LogInformation($"Starting edit transaction for settings...");
    }

    protected async Task CommitEditSettings(AdminMessageFactory adminMessageFactory)
    {
        var message = adminMessageFactory.CreateCommitEditSettingsMessage();
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message),
            AnyResponseReceived);
        Logger.LogInformation($"Commit edit transaction for settings...");
    }
}