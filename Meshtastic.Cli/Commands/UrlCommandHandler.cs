using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Enums;
using Meshtastic.Protobufs;
using Meshtastic.Display;

namespace Meshtastic.Cli.Commands;

public class UrlCommandHandler : DeviceCommandHandler
{
    private readonly UrlOperation operation;
    private readonly string? url;

    public UrlCommandHandler(UrlOperation operation, string? url, DeviceConnectionContext context, CommandContext commandContext) : 
        base(context, commandContext) 
    {
        this.operation = operation;
        this.url = url;
    }

    public async Task Handle()
    {
        var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        if (this.operation == UrlOperation.Set)
            await SetChannelsFromUrl(container);
        else if (operation == UrlOperation.Get)
        {
            var printer = new ProtobufPrinter(container, OutputFormat);
            printer.PrintUrl();
        }
    }

    private async Task SetChannelsFromUrl(DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        await BeginEditSettings(adminMessageFactory);

        var urlParser = new UrlParser(this.url!);
        var channelSet = urlParser.Parse();
        int index = 0;
        foreach (var setting in channelSet.Settings)
        {
            var channel = new Channel
            {
                Index = index,
                Role = index == 0 ? Channel.Types.Role.Primary : Channel.Types.Role.Secondary,
                Settings = setting
            };
            Logger.LogInformation($"Sending channel {index} to device...");
            var setChannel = adminMessageFactory.CreateSetChannelMessage(channel);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setChannel), 
                AnyResponseReceived);
            index++;
        }
        Logger.LogInformation("Sending LoRA config device...");

        var setLoraConfig = adminMessageFactory.CreateSetConfigMessage(channelSet.LoraConfig);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setLoraConfig), AnyResponseReceived);

        await CommitEditSettings(adminMessageFactory);
    }
}
