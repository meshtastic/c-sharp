using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.CommandHandlers;

public class InfoCommandHandler : DeviceCommandHandler
{
    public InfoCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container, OutputFormat);
        printer.Print();
        return Task.CompletedTask;
    }
}
