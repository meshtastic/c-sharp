using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Commands;

public class ListCommand : Command
{
    public ListCommand(string name, string description, Option<OutputFormat> _, Option<LogLevel> __) 
        : base(name, description)
    { 
        var listCommandHandler = new ListCommandHandler();
        this.SetHandler(ListCommandHandler.Handle);
    }
}
