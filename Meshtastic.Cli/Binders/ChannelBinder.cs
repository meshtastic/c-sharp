using Meshtastic.Cli.Enums;
using Meshtastic.Protobufs;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Cli.Binders;

public record ChannelOperationSettings(ChannelOperation Operation, int? Index, string? Name, Channel.Types.Role? Role, string? PSK, bool? UplinkEnabled, bool? DownlinkEnabled);

[ExcludeFromCodeCoverage(Justification = "Container object")]
public class ChannelBinder : BinderBase<ChannelOperationSettings>
{
    private readonly Argument<ChannelOperation> operation;
    private readonly Option<int> indexOption;
    private readonly Option<string?> nameOption;
    private readonly Option<Channel.Types.Role?> roleOption;
    private readonly Option<string?> pskOption;
    private readonly Option<bool?> uplinkOption;
    private readonly Option<bool?> downlinkOption;

    public ChannelBinder(Argument<ChannelOperation> operation,
        Option<int> indexOption,
        Option<string?> nameOption,
        Option<Channel.Types.Role?> roleOption,
        Option<string?> pskOption,
        Option<bool?> uplinkOption,
        Option<bool?> downlinkOption)
    {
        this.operation = operation;
        this.indexOption = indexOption;
        this.nameOption = nameOption;
        this.roleOption = roleOption;
        this.pskOption = pskOption;
        this.uplinkOption = uplinkOption;
        this.downlinkOption = downlinkOption;
    }

    protected override ChannelOperationSettings GetBoundValue(BindingContext bindingContext) =>
        new(bindingContext.ParseResult.GetValueForArgument(operation),
            bindingContext.ParseResult?.GetValueForOption(indexOption),
            bindingContext.ParseResult?.GetValueForOption(nameOption),
            bindingContext.ParseResult?.GetValueForOption(roleOption),
            bindingContext.ParseResult?.GetValueForOption(pskOption),
            bindingContext.ParseResult?.GetValueForOption(uplinkOption),
            bindingContext.ParseResult?.GetValueForOption(downlinkOption))
        {
        };
}
