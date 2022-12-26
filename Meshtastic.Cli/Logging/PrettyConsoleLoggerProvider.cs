using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Meshtastic.Cli.Logging;

internal class PrettyConsoleLoggerProvider : ILoggerProvider
{
    private readonly PrettyConsoleLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, PrettyConsoleLogger> _loggers = new();

    public PrettyConsoleLoggerProvider(PrettyConsoleLoggerConfiguration config) => _config = config;
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new PrettyConsoleLogger(_config));

    public void Dispose() => _loggers.Clear();
}