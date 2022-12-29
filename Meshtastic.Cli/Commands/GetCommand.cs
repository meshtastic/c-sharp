using Meshtastic.Display;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class GetCommand : Command
{
    public GetCommand(string name, string description, Option<IEnumerable<string>> settings, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest) : base(name, description)
    {
        this.SetHandler(async (settings, context, commandContext) =>
            {
                var handler = new GetCommandHandler(settings, context, commandContext);
                await handler.Handle();
            },
            settings,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest));
        this.AddOption(settings);
    }
}
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
