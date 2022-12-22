using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;

public class RebootCommand : Command
{
    public RebootCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var otaOption = new Option<bool>("ota", "Reboot into OTA update mode");
        otaOption.SetDefaultValue(false);

        var secondsArgument = new Argument<int>("seconds", "Number of seconds until reboot");
        secondsArgument.SetDefaultValue(10);

        var rebootCommandHandler = new RebootCommandHandler();
        this.SetHandler(rebootCommandHandler.Handle,
            otaOption,
            secondsArgument,
            new ConnectionBinder(port, host),
            new LoggingBinder());
        this.AddOption(otaOption);
        this.AddArgument(secondsArgument);
    }
}
public class RebootCommandHandler : DeviceCommandHandler
{
    private bool isOtaMode = false;
    private int seconds = 10;

    public async Task Handle(bool isOtaMode, int seconds, DeviceConnectionContext context, ILogger logger)
    {
        this.isOtaMode = isOtaMode;
        this.seconds = seconds;

        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
            var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        AnsiConsole.WriteLine($"Rebooting in {seconds} seconds...");
        var adminMessageFactory = new AdminMessageFactory(container);
        var adminMessage = adminMessageFactory.CreateRebootMessage(seconds, isOtaMode);
        await connection!.WriteToRadio(adminMessage.ToByteArray(), AlwaysComplete);
    }
}
