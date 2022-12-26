using Meshtastic.Display;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;

namespace Meshtastic.Cli.Commands;

public class GetCommand : Command
{
    public GetCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<IEnumerable<string>> settings) : base(name, description)
    {
        this.SetHandler(async (settings, context, outputFormat, logger) =>
            {
                var handler = new GetCommandHandler(settings, context, outputFormat, logger);
                await handler.Handle();
            },
            settings,
            new DeviceConnectionBinder(port, host),
            output,
            new LoggingBinder(log));
        this.AddOption(settings);
    }
}
public class GetCommandHandler : DeviceCommandHandler
{
    private readonly IEnumerable<ParsedSetting>? parsedSettings;

    public GetCommandHandler(IEnumerable<string> settings,
        DeviceConnectionContext context,
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger)
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
