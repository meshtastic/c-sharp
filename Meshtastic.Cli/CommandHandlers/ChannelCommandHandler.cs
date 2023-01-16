using Google.Protobuf;
using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Meshtastic.Cli.CommandHandlers;

public class ChannelCommandHandler : DeviceCommandHandler
{
    private readonly ChannelOperationSettings settings;

    public ChannelCommandHandler(ChannelOperationSettings settings,
        DeviceConnectionContext connectionContext,
        CommandContext commandContext) : base(connectionContext, commandContext)
    {
        this.settings = settings;
    }

    public async Task Handle()
    {
        var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
    }

    public override async Task OnCompleted(FromRadio fromRadio, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
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
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
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
