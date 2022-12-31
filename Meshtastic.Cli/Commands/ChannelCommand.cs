using Microsoft.Extensions.Logging;
using Meshtastic.Cli.Enums;
using Meshtastic.Cli.Binders;
using Meshtastic.Protobufs;

namespace Meshtastic.Cli.Commands;

public class ChannelCommand : Command
{
    public ChannelCommand(string name, string description, Option<string> port, Option<string> host, 
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : 
        base(name, description)
    {
        var commandContextBinder = new CommandContextBinder(log, output, dest, selectDest);
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

        this.SetHandler(async (settings, context, commandContext) =>
            {
                var handler = new ChannelCommandHandler(settings, context, commandContext);
                await handler.Handle();
            },
            channelBinder,
            new DeviceConnectionBinder(port, host),
            commandContextBinder);
    }
}
