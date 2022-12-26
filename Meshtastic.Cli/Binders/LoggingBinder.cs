using System.CommandLine.Binding;
using Meshtastic.Cli.Logging;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Binders;

public class LoggingBinder : BinderBase<ILogger>
{
    private readonly Option<LogLevel> logLevel;

    public LoggingBinder(Option<LogLevel> logLevel) => this.logLevel = logLevel;

    protected override ILogger GetBoundValue(BindingContext bindingContext) => GetLogger(bindingContext);

    public ILogger GetLogger(BindingContext bindingContext)
    {
        var level = bindingContext.ParseResult?.GetValueForOption(logLevel) ?? LogLevel.Information;
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddPrettyConsole(new PrettyConsoleLoggerConfiguration()
            {
                LogLevel = level,
            });
            builder.SetMinimumLevel(level);
        });
        return loggerFactory.CreateLogger<Program>();
    }
}
