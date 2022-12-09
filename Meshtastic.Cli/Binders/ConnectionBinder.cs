using System.CommandLine;
using System.CommandLine.Binding;
using Meshtastic.Connections;

namespace Meshtastic.Cli.Binders;

public class ConnectionBinder : BinderBase<DeviceConnectionContext>
{
    private readonly Option<string> portOption;
    private readonly Option<string> hostOption;

    public ConnectionBinder(Option<string> portOption, Option<string> hostOption)
    {
        this.portOption = portOption;
        this.hostOption = hostOption;
    }

    protected override DeviceConnectionContext GetBoundValue(BindingContext bindingContext) =>
        new DeviceConnectionContext(bindingContext.ParseResult?.GetValueForOption(portOption),
            bindingContext.ParseResult?.GetValueForOption(hostOption)) { };
}
