using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Logging;

internal class PrettyConsoleLoggerConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public int EventId { get; set; } = 0;
    public Action<IAnsiConsole>? ConsoleConfiguration { get; set; }
    public AnsiConsoleSettings? ConsoleSettings { get; set; } = null;
}
