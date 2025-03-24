using Meshtastic.Protobufs;

namespace Meshtastic.Simulator.Service.Persistance;

public interface ISimulatorStore
{
    Task Load();
    Task Save();

    User User { get; }
    LocalConfig LocalConfig { get; }
    LocalModuleConfig LocalModuleConfig { get; }
    List<Channel> Channels { get; }
    MyNodeInfo MyNodeInfo { get; }
    List<NodeInfo> Nodes { get; }
}
