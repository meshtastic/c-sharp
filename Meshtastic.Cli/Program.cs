using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Commands;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var port = new Option<string>("--port", description: "Target serial port for meshtastic device");
var host = new Option<string>("--host", description: "Target host ip or name for meshtastic device");

var output = new Option<OutputFormat>("--output", description: "Type of output format for the command");
output.AddCompletions(Enum.GetNames(typeof(OutputFormat)));

var log = new Option<LogLevel>("--log", description: "Logging level for command events");
log.AddAlias("-l");
log.SetDefaultValue(LogLevel.Information);
log.AddCompletions(Enum.GetNames(typeof(LogLevel)));

var setting = new Option<IEnumerable<string>>("--setting", description: "Get or set a value on config / module-config")
{
    AllowMultipleArgumentsPerToken = true,
};
setting.AddAlias("-s");
setting.AddCompletions(ctx => new LocalConfig().GetSettingsOptions().Concat(new LocalModuleConfig().GetSettingsOptions()));

var root = new RootCommand("Meshtastic.Cli");
root.AddGlobalOption(port);
root.AddGlobalOption(host);
root.AddGlobalOption(output);
root.AddGlobalOption(log);

root.AddCommand(new ListCommand("list", "List available serial ports", output, log));
root.AddCommand(new MonitorCommand("monitor", "Serial monitor for the device", port, host, output, log));
root.AddCommand(new InfoCommand("info", "Dump info about the device", port, host, output, log));
root.AddCommand(new GetCommand("get", "Display one or more settings from the device", port, host, output, log, setting));
root.AddCommand(new SetCommand("set", "Save one or more settings onto the device", port, host, output, log, setting));
root.AddCommand(new ChannelCommand("channel", "Enable, Disable, Add, Save channels on the device", port, host, output, log));
root.AddCommand(new UrlCommand("url", "Get or set shared channel url", port, host, output, log));
root.AddCommand(new RebootCommand("reboot", "Reboot the device", port, host, output, log));
root.AddCommand(new MetadataCommand("metadata", "Get device metadata from the device", port, host, output, log));
root.AddCommand(new FactoryResetCommand("factory-reset", "Factory reset configuration of the device", port, host, output, log));
root.AddCommand(new FixedPositionCommand("fixed-position", "Set the device to a fixed position", port, host, output, log));

var parser = new CommandLineBuilder(root)
    .UseExceptionHandler((ex, context) =>
    {
        var logging = new LoggingBinder(log);
        logging.GetLogger(context.BindingContext).LogError(ex, message: ex.Message);
    }, errorExitCode: 1)
    .UseVersionOption()
    .UseEnvironmentVariableDirective()
    .UseParseDirective()
    .UseSuggestDirective()
    .RegisterWithDotnetSuggest()
    .UseTypoCorrections()
    .UseParseErrorReporting()
    .CancelOnProcessTermination()
    .UseTypoCorrections()
    .UseHelp()
    .Build();

return await parser.InvokeAsync(args);