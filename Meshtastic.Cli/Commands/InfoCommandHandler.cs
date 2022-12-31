using Meshtastic.Data;
using Meshtastic.Display;

namespace Meshtastic.Cli.Commands;

public class InfoCommandHandler : DeviceCommandHandler
{
    public InfoCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext) { }
    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var printer = new ProtobufPrinter(container, OutputFormat);
        printer.Print();
        return Task.CompletedTask;
    }
}
