using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Logging;
using Microsoft.Extensions.Logging;
using System.CommandLine.Binding;

namespace Meshtastic.Cli.Binders;

public class CommandContextBinder : BinderBase<CommandContext>
{
    private readonly Option<LogLevel> logLevel;
    private readonly Option<OutputFormat> outputFormat;
    private readonly Option<uint?> destination;
    private readonly Option<bool> selectDest;
    private readonly Option<uint?>? channel;

    public CommandContextBinder(Option<LogLevel> logLevel,
        Option<OutputFormat> outputFormat,
        Option<uint?> destination,
        Option<bool> selectDest,
        Option<uint?>? channel = null)
    {
        this.logLevel = logLevel;
        this.outputFormat = outputFormat;
        this.destination = destination;
        this.selectDest = selectDest;
        this.channel = channel;
    }

    protected override CommandContext GetBoundValue(BindingContext bindingContext)
    {
        return new CommandContext(GetLogger(bindingContext),
            bindingContext.ParseResult?.GetValueForOption(outputFormat) ?? OutputFormat.PrettyConsole,
            bindingContext.ParseResult?.GetValueForOption(destination),
            bindingContext.ParseResult?.GetValueForOption(selectDest) ?? false,
            channel != null ? bindingContext.ParseResult?.GetValueForOption(channel) : null);
    }

    public ILogger GetLogger(BindingContext bindingContext)
    {
        var level = bindingContext.ParseResult?.GetValueForOption(logLevel) ?? LogLevel.Information;
        var output = bindingContext.ParseResult?.GetValueForOption(outputFormat) ?? OutputFormat.PrettyConsole;
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddPrettyConsole(new PrettyConsoleLoggerConfiguration()
            {
                // Don't allow non-console output formats to set chatty loglevels that will corrupt clean ouput
                LogLevel = output == OutputFormat.PrettyConsole ? level : LogLevel.Error,
            });
            builder.SetMinimumLevel(level);
        });
        return loggerFactory.CreateLogger<Program>();
    }
}
