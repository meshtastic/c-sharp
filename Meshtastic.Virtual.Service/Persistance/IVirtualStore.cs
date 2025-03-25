using Meshtastic.Protobufs;

namespace Meshtastic.Virtual.Service.Persistance;

public interface IVirtualStore
{
    Task Load();
    Task Save();

    NodeInfo Node { get; }
    LocalConfig LocalConfig { get; }
    LocalModuleConfig LocalModuleConfig { get; }
    List<Channel> Channels { get; }
    MyNodeInfo MyNodeInfo { get; }
    List<NodeInfo> Nodes { get; }
}
