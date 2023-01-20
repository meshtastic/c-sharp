using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class SendWaypointCommand : Command
{
    public SendWaypointCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var latArg = new Argument<decimal>("lat", description: "Latitude of the node (decimal format)");
        latArg.AddValidator(result =>
        {
            if (Math.Abs(result.GetValueForArgument(latArg)) > 90)
                result.ErrorMessage = "Invalid latitude";
        });
        AddArgument(latArg);

        var lonArg = new Argument<decimal>("lon", description: "Longitude of the waypoint (decimal format)");
        lonArg.AddValidator(result =>
        {
            if (Math.Abs(result.GetValueForArgument(lonArg)) > 180)
                result.ErrorMessage = "Invalid longitude";
        });
        AddArgument(lonArg);

        var nameOption = new Option<string>("--name", description: "Name for the waypoint");
        AddOption(nameOption);

        var descriptionOption = new Option<string>("--description", description: "Description for the waypoint");
        AddOption(descriptionOption);

        var iconOption = new Option<string>("--icon", description: "Icon emoji for the waypoint");
        iconOption.SetDefaultValue("📍");
        iconOption.AddValidator(ctx =>
        {
            var emoji = ctx.GetValueOrDefault()?.ToString();
            if (emoji == null)// || !Regex.IsMatch(emoji, EmojiRegexPattern))
            {
                ctx.ErrorMessage = "Must specifiy a valid emoji character for waypoint icon";
            }
        });
        AddOption(iconOption);

        var lockedOption = new Option<bool>("--locked", description: "Lock the waypoint from updates");
        AddOption(lockedOption);

        this.SetHandler(async (lat, lon, name, description, icon, locked, context, commandContext) =>
            {
                var handler = new SendWaypointCommandHandler(lat, lon, name, description, icon, locked, context, commandContext);
                await handler.Handle();
            },
            latArg,
            lonArg,
            nameOption,
            descriptionOption,
            iconOption,
            lockedOption,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
