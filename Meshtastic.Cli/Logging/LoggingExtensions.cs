using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Logging;

internal static class LoggingExtensions
{
    public static ILoggingBuilder AddPrettyConsole(this ILoggingBuilder loggingBuilder, PrettyConsoleLoggerConfiguration config)
    {
        loggingBuilder.AddProvider(new PrettyConsoleLoggerProvider(config));
        return loggingBuilder;
    }
}