using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Cli.CommandHandlers;

[ExcludeFromCodeCoverage(Justification = "Requires serial hardware")]
public class MonitorCommandHandler : DeviceCommandHandler
{
    public MonitorCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) { }
    public async Task Handle()
    {
        await Connection.Monitor();
    }
}
