using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli;

public class CommandContext
{
    public CommandContext(ILogger logger, OutputFormat outputFormat, uint? destination = null, bool selectDestination = false)
    {
        Logger = logger;
        OutputFormat = outputFormat;
        Destination = destination;
        SelectDestination = selectDestination;
    }

    public ILogger Logger { get; }
    public OutputFormat OutputFormat { get; }
    public uint? Destination { get; }
    public bool SelectDestination { get; }

}