using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Parsers;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Display;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class UrlCommandHandler : DeviceCommandHandler
{
    private readonly GetSetOperation operation;
    private readonly string? url;

    public UrlCommandHandler(GetSetOperation operation, string? url, DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext)
    {
        this.operation = operation;
        this.url = url;
    }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        if (operation == GetSetOperation.Set && url!.Contains("/v/#")) {
            await SetContactFromUrl(container);
        } else if (operation == GetSetOperation.Set) {
            await SetChannelsFromUrl(container);
        }
        else if (operation == GetSetOperation.Get)
        {
            var printer = new ProtobufPrinter(container, OutputFormat);
            printer.PrintUrl();
        }
    }
    private async Task SetContactFromUrl(DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        var urlParser = new UrlParser(url!);
        var contact = urlParser.ParseContact();
        Logger.LogInformation($"Sending contact {contact.User.LongName} to device...");
        var setContact = adminMessageFactory.CreateAddContactMessage(contact);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setContact), AnyResponseReceived);
    }

    private async Task SetChannelsFromUrl(DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        await BeginEditSettings(adminMessageFactory);
        var urlParser = new UrlParser(url!);
        var channelSet = urlParser.ParseChannels();
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
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setChannel), AnyResponseReceived);
            index++;
        }
        Logger.LogInformation("Sending LoRA config to device...");

        var setLoraConfig = adminMessageFactory.CreateSetConfigMessage(channelSet.LoraConfig);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setLoraConfig), AnyResponseReceived);

        await CommitEditSettings(adminMessageFactory);
    }
}
