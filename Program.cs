using System.CommandLine;
using Meshtastic.Handlers;

var portOption = new Option<string>(
    name: "--port",
    description: "Target serial port for meshtastic device");

var noProtoCommand = new Command("noproto", "Serial monitor for meshtastic devices");
noProtoCommand.SetHandler(NoProtoHandler.Handle, portOption);

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddCommand(noProtoCommand);

return await rootCommand.InvokeAsync(args);