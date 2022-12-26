using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class SetCommand : Command
{
    public SetCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<IEnumerable<string>> settings) : 
        base(name, description)
    {
        this.SetHandler(async (settings, context, outputFormat, logger) =>
            {
                var handler = new SetCommandHandler(settings, context, outputFormat, logger);
                await handler.Handle();
            },
            settings,
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
        this.AddOption(settings);
    }
}
public class SetCommandHandler : DeviceCommandHandler
{
    private readonly IEnumerable<ParsedSetting>? parsedSettings;
    public SetCommandHandler(IEnumerable<string> settings,
        DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger)
    {
        var (result, isValid) = ParseSettingOptions(settings, isGetOnly: false);
        if (!isValid)
            return;

        parsedSettings = result!.ParsedSettings;
    }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
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
            Logger.LogInformation($"Setting {setting.Section.Name}.{setting.Setting.Name} to {setting.Value?.ToString() ?? String.Empty}...");
        }
        await CommitEditSettings(adminMessageFactory);
    }

    private async Task SetModuleConfig(DeviceStateContainer container, AdminMessageFactory adminMessageFactory, ParsedSetting setting)
    {
        var instance = setting.Section.GetValue(container.LocalModuleConfig);
        setting.Setting.SetValue(instance, setting.Value);
        var adminMessage = adminMessageFactory.CreateSetModuleConfigMessage(instance!);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            AnyResponseReceived);
    }

    private async Task SetConfig(DeviceStateContainer container, AdminMessageFactory adminMessageFactory, ParsedSetting setting)
    {
        var instance = setting.Section.GetValue(container.LocalConfig);
        setting.Setting.SetValue(instance, setting.Value);
        var adminMessage = adminMessageFactory.CreateSetConfigMessage(instance!);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage),
            AnyResponseReceived);
    }
}
