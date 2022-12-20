using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Binders;
using Meshtastic.Protobufs;
using System.Security.Cryptography;
using System.Text;

namespace Meshtastic.Cli.Commands;

public class ChannelCommand : Command
{
    public ChannelCommand(string name, string description, Option<string> port, Option<string> host) : base(name, description)
    {
        var operationArgument = new Argument<ChannelOperation>("operation", "The type of channel operation");
        operationArgument.AddCompletions(ctx => Enum.GetNames(typeof(ChannelOperation)));

        var indexOption = new Option<int?>("--index", description: "Channel index");
        indexOption.AddAlias("-i");
        var nameOption = new Option<string?>("--name", description: "Channel name");
        nameOption.AddAlias("-n");
        var roleOption = new Option<Channel.Types.Role?>("--role", description: "Channel role");
        roleOption.AddAlias("-r");
        var pskOption = new Option<string?>("--psk", description: "Channel pre-shared key");
        pskOption.AddAlias("-p");
        var uplinkOption = new Option<bool?>("--uplink-enabled", description: "Channel uplink enabled");
        uplinkOption.AddAlias("-u");
        var downlinkOption = new Option<bool?>("--downlink-enabled", description: "Channel downlink enabled");
        downlinkOption.AddAlias("-d");
        var channelBinder = new ChannelBinder(operationArgument, indexOption, nameOption, roleOption, pskOption, uplinkOption, downlinkOption);
        
        var channelCommandHandler = new ChannelCommandHandler();
        this.SetHandler(channelCommandHandler.Handle, 
            channelBinder, 
            new ConnectionBinder(port, host), 
            new LoggingBinder());

        AddArgument(operationArgument);
        AddOption(indexOption);
        AddOption(nameOption);
        AddOption(roleOption);
        AddOption(pskOption);
        AddOption(uplinkOption);
        AddOption(downlinkOption);
    }
}

public class ChannelCommandHandler : DeviceCommandHandler
{
    private ChannelOperationSettings? _settings;
    public async Task Handle(ChannelOperationSettings settings, DeviceConnectionContext context, ILogger logger)
    {
        _settings = settings;
        if (settings.Index.HasValue && (settings.Index.Value > 8 || settings.Index.Value < 0))
        {
            AnsiConsole.WriteLine("[red]Channel index is out of range[/]");
            return;
        }
        if (settings.Operation != ChannelOperation.Add && !settings.Index.HasValue)
        {
            AnsiConsole.WriteLine($"[red]Must specify an index for this {settings.Operation} operation[/]");
            return; 
        }
        if ((settings.Operation == ChannelOperation.Disable || settings.Operation == ChannelOperation.Enable) && 
            settings.Index == 0) 
        {
            AnsiConsole.WriteLine($"[red]Cannot enable / disable PRIMARY channel[/]");
            return;
        }

        await OnConnection(context, async () =>
        {
            connection = context.GetDeviceConnection();
            var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();

            await connection.WriteToRadio(wantConfig.ToByteArray(), DefaultIsCompleteAsync);
        });
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        if (_settings == null)
            throw new InvalidOperationException("Cannot complete ChannelCommandHandler without ChannelOperationSettings");

        var adminMessageFactory = new AdminMessageFactory(container);
        await BeginEditSettings(adminMessageFactory);

        var channel = container.Channels.Find(c => c.Index == _settings.Index);

        AnsiConsole.MarkupLine("Writing channel");

        switch (_settings.Operation)
        {
            case ChannelOperation.Add:
                container.Channels.Find(c => c.Role == Channel.Types.Role.Disabled);
                SetChannelSettings(_settings, channel);
                break;
            case ChannelOperation.Disable:
                if (channel != null) channel.Role = Channel.Types.Role.Disabled;
                break;
            case ChannelOperation.Save:
                SetChannelSettings(_settings, channel);
                break;
            case ChannelOperation.Enable:
                if (channel != null) channel.Role = Channel.Types.Role.Primary;
                break;
            default:
                throw new InvalidOperationException("Cannot complete ChannelCommandHandler without ChannelOperation");
        }
        var adminMessage = adminMessageFactory.CreateSetChannelMessage(channel!);
        await connection!.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage).ToByteArray(), 
            AlwaysComplete);
        await CommitEditSettings(adminMessageFactory);
    }

    private static void SetChannelSettings(ChannelOperationSettings settings, Channel? channel)
    {
        if (channel != null)
        {
            if (channel.Index > 0)
                channel.Role = settings.Role ?? Channel.Types.Role.Secondary;
            if (settings.Name != null)
                channel.Settings.Name = settings.Name;
            if (settings.DownlinkEnabled.HasValue)
                channel.Settings.DownlinkEnabled = settings.DownlinkEnabled.Value;
            if (settings.UplinkEnabled.HasValue)
                channel.Settings.UplinkEnabled = settings.UplinkEnabled.Value;
            if (settings.PSK != null)
            {
                if (settings.PSK == "none")
                    channel.Settings.Psk = ByteString.Empty;
                else if (settings.PSK == "random")
                {
                    using var random = RandomNumberGenerator.Create();
                    byte[] data = new byte[32];
                    random.GetBytes(data);
                    channel.Settings.Psk = ByteString.CopyFrom(data);
                }
                else
                {
                    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(settings.PSK));
                    channel.Settings.Psk = ByteString.CopyFrom(hash);
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Could not find available channel[/]");
        }
    }
}
