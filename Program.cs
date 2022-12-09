using System.CommandLine;
using System.CommandLine.Binding;
using Microsoft.Extensions.Logging;

using Meshtastic;
using Meshtastic.Handlers;
using Spectre.Console;
using Meshtastic.Connections;

var portOption = new Option<string>(name: "--port", description: "Target serial port for meshtastic device");
var hostOption = new Option<string>(name: "--host", description: "Target host ip or name for meshtastic device");

var noProtoCommand = new Command("monitor", "Serial monitor for meshtastic devices");
noProtoCommand.SetHandler(MonitorHandler.Handle, 
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var infoCommand = new Command("info", "Dump info about the currently connected meshtastic node");
infoCommand.SetHandler(InfoCommandHandler.Handle, 
    new ConnectionBinder(portOption, hostOption), 
    new LoggingBinder());

var rootCommand = new RootCommand("Meshtastic CLI");
rootCommand.AddGlobalOption(portOption);
rootCommand.AddGlobalOption(hostOption);
rootCommand.AddCommand(noProtoCommand);
rootCommand.AddCommand(infoCommand);

// return await AnsiConsole.Status()
//     .StartAsync("Connecting...", async ctx => 
//     {
//         ctx.Status("Connecting...");
//         ctx.Spinner(Spinner.Known.Dots);
//         ctx.SpinnerStyle(new Style(Resources.MESHTASTIC_GREEN));

        return await rootCommand.InvokeAsync(args);   
    //});


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

public class ConnectionBinder : BinderBase<DeviceConnectionContext>
{
    private readonly Option<string> portOption;
    private readonly Option<string> hostOption;

    public ConnectionBinder(Option<string> portOption, Option<string> hostOption)
    {
        this.portOption = portOption;
        this.hostOption = hostOption;
    }

    protected override DeviceConnectionContext GetBoundValue(BindingContext bindingContext) =>
        new DeviceConnectionContext(bindingContext.ParseResult?.GetValueForOption(portOption),
            bindingContext.ParseResult?.GetValueForOption(hostOption)) { };
}
