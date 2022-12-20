using Meshtastic.Cli.Commands;
using Meshtastic.Cli.Reflection;
using Meshtastic.Protobufs;

var portOption = new Option<string>("--port", description: "Target serial port for meshtastic device");
var hostOption = new Option<string>("--host", description: "Target host ip or name for meshtastic device");

var settingOption = new Option<IEnumerable<string>>("--setting", description: "Get or set a value on config / module-config")
{
    AllowMultipleArgumentsPerToken = true,
};
settingOption.AddAlias("-s");
settingOption.AddCompletions((ctx) =>
    new LocalConfig().GetSettingsOptions().Concat(new LocalModuleConfig().GetSettingsOptions()));

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddGlobalOption(hostOption);

rootCommand.AddCommand(new ListCommand("list", "List available serial devices", portOption, hostOption));
rootCommand.AddCommand(new MonitorCommand("monitor", "Serial monitor for meshtastic devices", portOption, hostOption));
rootCommand.AddCommand(new InfoCommand("info", "Dump info about the currently connected meshtastic node", portOption, hostOption));
rootCommand.AddCommand(new GetCommand("get", "Display one or more settings from the connected device", portOption, hostOption, settingOption));
rootCommand.AddCommand(new SetCommand("set", "Save one or more settings onto the connected device", portOption, hostOption, settingOption));
rootCommand.AddCommand(new ChannelCommand("channel", "Enable, Disable, Add, Save channels", portOption, hostOption));
rootCommand.AddCommand(new UrlCommand("url", "Get or set shared channel url", portOption, hostOption));
rootCommand.AddCommand(new RebootCommand("reboot", "Reboot the meshtastic node", portOption, hostOption));

return await rootCommand.InvokeAsync(args);