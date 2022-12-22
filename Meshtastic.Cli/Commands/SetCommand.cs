using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;

public class SetCommand : Command
{
    public SetCommand(string name, string description, Option<string> port, Option<string> host, Option<IEnumerable<string>> settings) :
        base(name, description)
    {
        var setCommandHandler = new SetCommandHandler();
        this.SetHandler(setCommandHandler.Handle,
            settings,
            new ConnectionBinder(port, host),
            new LoggingBinder());
        this.AddOption(settings);
    }
}
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
        await BeginEditSettings(adminMessageFactory);

        foreach (var setting in parsedSettings!)
        {
            if (setting.Section.ReflectedType?.Name == nameof(container.LocalConfig))
                await SetConfig(container, adminMessageFactory, setting);
            else
                await SetModuleConfig(container, adminMessageFactory, setting);
            AnsiConsole.MarkupLine($"Setting {setting.Section.Name}.{setting.Setting.Name} to {setting.Value?.ToString() ?? String.Empty}...");
        }
        await CommitEditSettings(adminMessageFactory);
    }

    private async Task SetModuleConfig(DeviceStateContainer container, AdminMessageFactory adminMessageFactory, ParsedSetting setting)
    {
        var instance = setting.Section.GetValue(container.LocalModuleConfig);
        setting.Setting.SetValue(instance, setting.Value);
        var adminMessage = adminMessageFactory.CreateSetModuleConfigMessage(instance!);
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage).ToByteArray(),
            AlwaysComplete);
    }

    private async Task SetConfig(DeviceStateContainer container, AdminMessageFactory adminMessageFactory, ParsedSetting setting)
    {
        var instance = setting.Section.GetValue(container.LocalConfig);
        setting.Setting.SetValue(instance, setting.Value);
        var adminMessage = adminMessageFactory.CreateSetConfigMessage(instance!);
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage).ToByteArray(),
            AlwaysComplete);
    }
}
