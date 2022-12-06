namespace Meshtastic.Handlers;

public interface ICommandHandler<TArg> 
{
    abstract static Task Handle(TArg arg);
}