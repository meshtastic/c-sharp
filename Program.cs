using System.CommandLine;
using System.CommandLine.Binding;
using Microsoft.Extensions.Logging;

using Meshtastic.Handlers;
using Spectre.Console;

var portOption = new Option<string>(name: "--port", description: "Target serial port for meshtastic device");
var hostOption = new Option<string>(name: "--host", description: "Target host ip or name for meshtastic device");

var noProtoCommand = new Command("noproto", "Serial monitor for meshtastic devices");
noProtoCommand.SetHandler(NoProtoHandler.Handle, portOption, new LoggingBinder());

var infoCommand = new Command("info", "Dump info about the currently connected meshtastic node");
infoCommand.SetHandler(InfoCommandHandler.Handle, portOption, new LoggingBinder());

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddCommand(noProtoCommand);
rootCommand.AddCommand(infoCommand);

return await rootCommand.InvokeAsync(args);

public class LoggingBinder : BinderBase<ILogger>
{
    protected override ILogger GetBoundValue(BindingContext bindingContext) => GetLogger(bindingContext);

    ILogger GetLogger(BindingContext bindingContext)
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = loggerFactory.CreateLogger("LoggerCategory");
        return logger;
    }
}