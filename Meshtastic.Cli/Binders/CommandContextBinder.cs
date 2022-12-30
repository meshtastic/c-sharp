using System;
using System.CommandLine.Binding;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Logging;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Binders;

public class CommandContextBinder : BinderBase<CommandContext>
{
    private readonly Option<LogLevel> logLevel;
    private readonly Option<OutputFormat> outputFormat;
    private readonly Option<uint?> destination;
    private readonly Option<bool> selectDest;

    public CommandContextBinder(Option<LogLevel> logLevel, Option<OutputFormat> outputFormat, Option<uint?> destination, Option<bool> selectDest)
    {
        this.logLevel = logLevel;
        this.outputFormat = outputFormat;
        this.destination = destination;
        this.selectDest = selectDest;
    }

    protected override CommandContext GetBoundValue(BindingContext bindingContext)
    {
        return new CommandContext(GetLogger(bindingContext),
            bindingContext.ParseResult?.GetValueForOption(outputFormat) ?? OutputFormat.Console,
            bindingContext.ParseResult?.GetValueForOption(destination),
            bindingContext.ParseResult?.GetValueForOption(selectDest) ?? false);
    }

    public ILogger GetLogger(BindingContext bindingContext)
    {
        var level = bindingContext.ParseResult?.GetValueForOption(logLevel) ?? LogLevel.Information;
        var output = bindingContext.ParseResult?.GetValueForOption(outputFormat) ?? OutputFormat.Console;
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddPrettyConsole(new PrettyConsoleLoggerConfiguration()
            {
                // Don't allow non-console output formats to set chatty loglevels that will corrupt clean ouput
                LogLevel = output != OutputFormat.Console ? level : LogLevel.Error,
            });
            builder.SetMinimumLevel(level);
        });
        return loggerFactory.CreateLogger<Program>();
    }
}
