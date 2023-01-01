namespace Meshtastic.Cli.CommandHandlers;

public class MonitorCommandHandler : DeviceCommandHandler
{
    public MonitorCommandHandler(DeviceConnectionContext context,
        CommandContext commandContext) : base(context, commandContext) { }
    public async Task Handle()
    {
        await Connection.Monitor();
    }
}
