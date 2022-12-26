using System.CommandLine.Binding;

namespace Meshtastic.Cli.Binders;

public class DeviceConnectionBinder : BinderBase<DeviceConnectionContext>
{
    private readonly Option<string> portOption;
    private readonly Option<string> hostOption;

    public DeviceConnectionBinder(Option<string> portOption, Option<string> hostOption)
    {
        this.portOption = portOption;
        this.hostOption = hostOption;
    }

    protected override DeviceConnectionContext GetBoundValue(BindingContext bindingContext) =>
        new(bindingContext.ParseResult?.GetValueForOption(portOption), 
            bindingContext.ParseResult?.GetValueForOption(hostOption));
}
