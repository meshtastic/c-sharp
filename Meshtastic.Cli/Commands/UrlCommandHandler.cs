using Meshtastic.Connections;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Enums;
using Meshtastic.Protobufs;
using Meshtastic.Display;
using Meshtastic.Cli.Binders;

namespace Meshtastic.Cli.Commands;

public class UrlCommand : Command
{
    public UrlCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var urlOperationArgument = new Argument<UrlOperation>("operation", "The type of url operation");
        urlOperationArgument.AddCompletions(ctx => Enum.GetNames(typeof(UrlOperation)));
        var urlArgument = new Argument<string?>("url", "The channel url to set on the device");
        urlArgument.SetDefaultValue(null);

        var urlCommandHandler = new UrlCommandHandler();
        this.SetHandler(urlCommandHandler.Handle,
            urlOperationArgument,
            urlArgument,
            new ConnectionBinder(port, host),
            new LoggingBinder());
        this.AddArgument(urlOperationArgument);
        this.AddArgument(urlArgument);
    }
}

public class UrlCommandHandler : DeviceCommandHandler
{
    private UrlOperation _operation;
    private string? _url;

    public async Task Handle(UrlOperation operation, string? url, DeviceConnectionContext context, ILogger logger)
    {
        _operation = operation;
        _url = url;

        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
            var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        if (_operation == UrlOperation.Set)
            await SetChannelsFromUrl(container);
        else if (_operation == UrlOperation.Get)
        {
            var printer = new ProtobufPrinter(container);
            printer.PrintUrl();
        }
    }

    private async Task SetChannelsFromUrl(DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container);
        await BeginEditSettings(adminMessageFactory);

        var urlParser = new UrlParser(_url!);
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
            AnsiConsole.MarkupLine($"Sending channel {index} to device...");
            var setChannel = adminMessageFactory.CreateSetChannelMessage(channel);
            //AnsiConsole.MarkupLine(setChannel.ToString());
            await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setChannel).ToByteArray(), 
                AlwaysComplete);
            index++;
        }
        AnsiConsole.MarkupLine("Sending LoRA config device...");

        var setLoraConfig = adminMessageFactory.CreateSetConfigMessage(channelSet.LoraConfig);
        //AnsiConsole.MarkupLine(setLoraConfig.ToString());
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setLoraConfig).ToByteArray(), AlwaysComplete);

        await CommitEditSettings(adminMessageFactory);
    }
}
