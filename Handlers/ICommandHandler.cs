using Microsoft.Extensions.Logging;

namespace Meshtastic.Handlers;

public interface ICommandHandler<TArgument> 
{
    abstract static Task Handle(TArgument arg, ILogger logger);
}