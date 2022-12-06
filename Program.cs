using System.CommandLine;
using Meshtastic.Handlers;

var portOption = new Option<string>(
    name: "--port",
    description: "Target serial port for meshtastic device");

var noProtoCommand = new Command("noproto", "Serial monitor for meshtastic devices");
noProtoCommand.SetHandler(NoProtoHandler.Handle, portOption);

var infoCommand = new Command("info", "Dump info about the currently connected meshtastic node");
infoCommand.SetHandler(InfoCommandHandler.Handle, portOption);

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddCommand(noProtoCommand);
rootCommand.AddCommand(infoCommand);

return await rootCommand.InvokeAsync(args);