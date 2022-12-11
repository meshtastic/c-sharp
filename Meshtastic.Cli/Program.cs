using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Handlers;

var portOption = new Option<string>(name: "--port", description: "Target serial port for meshtastic device");
var hostOption = new Option<string>(name: "--host", description: "Target host ip or name for meshtastic device");

var settingOption = new Option<IEnumerable<string>>(name: "--setting", description: "Get or set a value on config / module-config")
{
    AllowMultipleArgumentsPerToken = true,
};
settingOption.AddAlias("-s");
//settingOption.AddCompletions((ctx) => 
//    typeof(LocalConfig).GetSettingsOptions()
//    .Concat(typeof(LocalModuleConfig).GetSettingsOptions()));

// meshtastic get --pref mqtt.enable=true --pref mqtt.host=mqtt.meshtastic.org
// meshtastic set --pref mqtt.enable=true --pref mqtt.host=mqtt.meshtastic.org
// meshtastic set -p mqtt.enable=true -p mqtt.host=mqtt.meshtastic.org

//ping-text "Payload" --interval 2

var monitorCommand = new Command("monitor", "Serial monitor for meshtastic devices");
var monitorCommandHandler = new MonitorCommandHandler();
monitorCommand.SetHandler(monitorCommandHandler.Handle,
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var infoCommand = new Command("info", "Dump info about the currently connected meshtastic node");
var infoCommandHandler = new InfoCommandHandler();
infoCommand.SetHandler(infoCommandHandler.Handle,
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var getCommand = new Command("get", "Display one or more settings from the connected device");
var getCommandHandler = new GetCommandHandler();
getCommand.SetHandler(getCommandHandler.Handle,
    settingOption,
    new ConnectionBinder(portOption, hostOption),
    new LoggingBinder());
getCommand.AddOption(settingOption);

var setCommand = new Command("set", "Save one or more settings onto the connected device");
var setCommandHandler = new GetCommandHandler();
setCommand.SetHandler(setCommandHandler.Handle,
    settingOption,
    new ConnectionBinder(portOption, hostOption),
    new LoggingBinder());

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddGlobalOption(hostOption);
rootCommand.AddCommand(monitorCommand);
rootCommand.AddCommand(infoCommand);
rootCommand.AddCommand(getCommand);
rootCommand.AddCommand(setCommand);

return await AnsiConsole.Status()
    .StartAsync("Connecting...", async ctx =>
    {
        ctx.Status("Connecting...");
        ctx.Spinner(Spinner.Known.Dots);
        ctx.SpinnerStyle(new Style(Meshtastic.Cli.StyleResources.MESHTASTIC_GREEN));

        return await rootCommand.InvokeAsync(args);
    });
