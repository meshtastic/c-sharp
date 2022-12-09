using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.Handlers;

public interface ICommandHandler<TArgument> 
{
    abstract static Task Handle(TArgument arg, ILogger logger);
}