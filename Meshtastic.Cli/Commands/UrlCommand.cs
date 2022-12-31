using Meshtastic.Cli.Binders;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class UrlCommand : Command
{
    public UrlCommand(string name, string description, Option<string> port, Option<string> host,
        Option<OutputFormat> output, Option<LogLevel> log) : base(name, description)
    {
        var urlOperationArgument = new Argument<UrlOperation>("operation", "The type of url operation");
        urlOperationArgument.AddCompletions(ctx => Enum.GetNames(typeof(UrlOperation)));
        var urlArgument = new Argument<string?>("url", "The channel url to set on the device");
        urlArgument.SetDefaultValue(null);

        this.SetHandler(async (operation, url, context, commandContext) =>
            {
                var handler = new UrlCommandHandler(operation, url, context, commandContext);
                await handler.Handle();
            },
            urlOperationArgument,
            urlArgument,
            new DeviceConnectionBinder(port, host),
            new CommandContextBinder(log, output, new Option<uint?>("dest") { }, new Option<bool>("select-dest") { }));
        this.AddArgument(urlOperationArgument);
        this.AddArgument(urlArgument);
    }
}
