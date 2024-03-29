﻿using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class SetCannedMessagesCommandHandler : DeviceCommandHandler
{
    private readonly string messages;

    public SetCannedMessagesCommandHandler(string messages, DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext)
    {
        this.messages = messages;
    }
    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        await BeginEditSettings(adminMessageFactory);
        Logger.LogInformation($"Setting canned messages on device...");

        int index = 0;
        do
        {
            var upperBound = index + 200 > messages.Length ? messages.Length : index + 200;
            var setCannedMessage = adminMessageFactory.CreateSetCannedMessage(messages[index..upperBound]);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setCannedMessage), AnyResponseReceived);

            index = upperBound + 1;
        } while (index < (messages.Length - 200));

        await CommitEditSettings(adminMessageFactory);
    }
}
