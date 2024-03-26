using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class FixedPositionCommand : Command
{
    public FixedPositionCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log, Option<uint?> dest, Option<bool> selectDest) : base(name, description)
    {
        var latArg = new Argument<decimal>("lat", description: "Latitude of the node (decimal format)");
        latArg.AddValidator(result =>
        {
            if (Math.Abs(result.GetValueForArgument(latArg)) > 90)
                result.ErrorMessage = "Invalid latitude";
        });
        AddArgument(latArg);

        var lonArg = new Argument<decimal>("lon", description: "Longitude of the node (decimal format)");
        lonArg.AddValidator(result =>
        {
            if (Math.Abs(result.GetValueForArgument(lonArg)) > 180)
                result.ErrorMessage = "Invalid longitude";
        });
        AddArgument(lonArg);

        var altArg = new Argument<int>("altitude", description: "Altitude of the node (meters)");
        altArg.SetDefaultValue(0);
        AddArgument(altArg);

        var clearOption = new Option<bool>("clear", description: "Clear fixed position");
        clearOption.SetDefaultValue(false);
        AddOption(clearOption);

        this.SetHandler(async (lat, lon, alt, clear, context, commandContext) =>
            {
                var handler = new FixedPositionCommandHandler(lat, lon, alt, clear, context, commandContext);
                await handler.Handle();
            },
            latArg,
            lonArg,
            altArg,
            clearOption,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, dest, selectDest));
    }
}
