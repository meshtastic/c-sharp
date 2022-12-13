using Meshtastic.Connections;
using Meshtastic.Protobufs;
using Meshtastic.Display;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using System.ComponentModel;
using System.Net.Sockets;
using Spectre.Console;
using Google.Protobuf.WellKnownTypes;

namespace Meshtastic.Cli.Handlers;

public class SetCommandHandler : DeviceCommandHandler
{
    private IEnumerable<ParsedSetting>? parsedSettings;

    public async Task Handle(IEnumerable<string> settings, DeviceConnectionContext context, ILogger logger)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: false);
        if (!isValid)
            return;

        parsedSettings = result!.ParsedSettings;

        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
            var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container);
        var message = adminMessageFactory.CreateBeginEditSettingsMessage();

        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(message).ToByteArray(),
            AlwaysComplete);
        AnsiConsole.MarkupLine($"[olive]Starting edit transaction for settings...[/]");

        foreach (var setting in parsedSettings!)
        {
            if (setting.Section.ReflectedType?.Name == nameof(container.LocalConfig))
            {
                var instance = setting.Section.GetValue(container.LocalConfig);
                setting.Setting.SetValue(instance, setting.Value);
                await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessageFactory.CreateSetConfigMessage(instance!)).ToByteArray(), 
                    AlwaysComplete);
            }
            else
            {
                var instance = setting.Section.GetValue(container.LocalModuleConfig);
                setting.Setting.SetValue(instance, setting.Value);
                await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessageFactory.CreateSetModuleConfigMessage(instance!)).ToByteArray(),
                    AlwaysComplete);
            }
            AnsiConsole.MarkupLine($"Setting {setting.Section.Name}.{setting.Setting.Name} to {setting.Value?.ToString() ?? String.Empty}...");
        }
        AnsiConsole.MarkupLine($"[green]Commiting edit transaction for settings...[/]");
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessageFactory.CreateCommitEditSettingsMessage()).ToByteArray(), AlwaysComplete);
    }
}
