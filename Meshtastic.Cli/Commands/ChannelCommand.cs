using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Meshtastic.Data;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Binders;
using Meshtastic.Protobufs;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace Meshtastic.Cli.Commands;

public class ChannelCommand : Command
{
    public ChannelCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        var loggingBinder = new LoggingBinder(log);
        var operationArgument = new Argument<ChannelOperation>("operation", "The type of channel operation");
        operationArgument.AddCompletions(ctx => Enum.GetNames(typeof(ChannelOperation)));

        var indexOption = new Option<int>("--index", description: "Channel index");
        indexOption.AddAlias("-i");
        indexOption.SetDefaultValue(0);
        indexOption.AddValidator(context =>
        {
            var nonIndexZeroOperation = new [] { ChannelOperation.Disable, ChannelOperation.Enable };
            if (context.GetValueForOption(indexOption) < 0 || context.GetValueForOption(indexOption) > 8)
                context.ErrorMessage = "Channel index is out of range (0-8)";
            else if (nonIndexZeroOperation.Contains(context.GetValueForArgument(operationArgument)) &&
                context.GetValueForOption(indexOption) == 0) 
            {
                context.ErrorMessage = "Cannot enable / disable PRIMARY channel";
            }
        });
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

        AddArgument(operationArgument);
        AddOption(indexOption);
        AddOption(nameOption);
        AddOption(roleOption);
        AddOption(pskOption);
        AddOption(uplinkOption);
        AddOption(downlinkOption);

        this.SetHandler(async (settings, context, outputFormat, logger) =>
            {
                var handler = new ChannelCommandHandler(settings, context, outputFormat, logger);
                await handler.Handle();
            },
            channelBinder,
            new DeviceConnectionBinder(port, host),
            output,
            loggingBinder);
    }
}

public class ChannelCommandHandler : DeviceCommandHandler
{
    private readonly ChannelOperationSettings settings;

    public ChannelCommandHandler(ChannelOperationSettings settings, 
        DeviceConnectionContext context, 
        OutputFormat outputFormat,
        ILogger logger) : base(context, outputFormat, logger) 
        {
            this.settings = settings;
        }

    public async Task Handle()
    {
        var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromDeviceMessage packet, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container);
        await BeginEditSettings(adminMessageFactory);

        var channel = container.Channels.Find(c => c.Index == settings.Index);

        AnsiConsole.MarkupLine("Writing channel");

        switch (settings.Operation)
        {
            case ChannelOperation.Add:
                container.Channels.Find(c => c.Role == Channel.Types.Role.Disabled);
                SetChannelSettings(channel);
                break;
            case ChannelOperation.Disable:
                if (channel != null) channel.Role = Channel.Types.Role.Disabled;
                break;
            case ChannelOperation.Save:
                SetChannelSettings(channel);
                break;
            case ChannelOperation.Enable:
                if (channel != null) channel.Role = Channel.Types.Role.Primary;
                break;
            default:
                throw new UnreachableException("Cannot complete ChannelCommandHandler without ChannelOperation");
        }
        var adminMessage = adminMessageFactory.CreateSetChannelMessage(channel!);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), 
            AnyResponseReceived);
        await CommitEditSettings(adminMessageFactory);
    }

    private void SetChannelSettings(Channel? channel)
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
            throw new IndexOutOfRangeException($"Could not find available channel with index {settings!.Index}");
        }
    }
}
