using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli;

public class CommandContext
{
    public CommandContext(ILogger logger, OutputFormat outputFormat, uint? destination = null)
    {
        Logger = logger;
        OutputFormat = outputFormat;
        Destination = destination;
    }

    public ILogger Logger { get; }
    public OutputFormat OutputFormat { get; }
    public uint? Destination { get; }
}