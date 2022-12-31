using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Logging;

internal class PrettyConsoleLogger : ILogger
{
    private readonly PrettyConsoleLoggerConfiguration config;
    private readonly IAnsiConsole console;

    public PrettyConsoleLogger(PrettyConsoleLoggerConfiguration config)
    {
        this.config = config;

        var settings = config.ConsoleSettings ?? new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
        };
        console = AnsiConsole.Create(settings);
        config.ConsoleConfiguration?.Invoke(console);
    }

#pragma warning disable CS8603 // Possible null reference return.
    IDisposable ILogger.BeginScope<TState>(TState state) => null;
#pragma warning restore CS8603 // Possible null reference return.

    public bool IsEnabled(LogLevel logLevel) => logLevel >= config.LogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var prefix = GetLevelMarkup(logLevel);
        var message = formatter(state, exception).EscapeMarkup();
        console.MarkupLine($"{prefix}{message}[/]");
    }

    private static string GetLevelMarkup(LogLevel level)
    {
        return level switch
        {
            LogLevel.Critical => "[bold underline white on red]",
            LogLevel.Error => "[bold red]",
            LogLevel.Warning => "[bold orange3]",
            LogLevel.Information => "[bold dim]",
            LogLevel.Debug => "[dim cyan1]",
            LogLevel.Trace => "[dim grey]",
            (LogLevel.None or _) => String.Empty,
        };
    }
}
