using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using Spectre.Console.Json;
using Newtonsoft.Json;

namespace Meshtastic.Cli.CommandHandlers;

public class RegisterCommandHandlerCommandHandler(DeviceConnectionContext context, CommandContext commandContext) : DeviceCommandHandler(context, commandContext)
{
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        Logger.LogInformation("Getting registration info...");
        var key = container.MyNodeInfo.DeviceId;
        var user = container.Nodes.Find(n => n.Num == container.MyNodeInfo.MyNodeNum)?.User;
#pragma warning disable CS0612 // Type or member is obsolete
        var macAddress = user?.Macaddr;
#pragma warning restore CS0612 // Type or member is obsolete
        if (key == null || key.All(b => b == 0) || user == null || macAddress == null)
        {
            Logger.LogError("Device does not have a valid key or mac address, and cannot be registered.");
            return;
        }
        var jsonForm = JsonConvert.SerializeObject(new
        {
            MeshtasticDeviceId = Convert.ToHexString(key.ToByteArray()),
            MACAddress = Convert.ToHexString(macAddress.ToByteArray()),
            DeviceHardwareId = container.Metadata.HwModel,
            container.Metadata.FirmwareVersion,
        });

        var json = new JsonText(jsonForm);

        AnsiConsole.Write( new Panel(json)
            .Header("Registration Information")
            .Collapse()
            .RoundedBorder()
            .BorderColor(Color.Blue));

        await Task.CompletedTask;
    }
}
