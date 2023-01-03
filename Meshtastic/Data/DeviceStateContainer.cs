using Meshtastic.Protobufs;
using System.ComponentModel;

namespace Meshtastic.Data;

public class DeviceStateContainer
{
    public LocalConfig LocalConfig;
    public LocalModuleConfig LocalModuleConfig;
    public List<Channel> Channels;
    public MyNodeInfo MyNodeInfo;
    public List<NodeInfo> Nodes;

    public DeviceStateContainer(LocalConfig localConfig,
        LocalModuleConfig localModuleConfig,
        List<Channel> channels,
        MyNodeInfo myNodeInfo,
        List<NodeInfo> nodes)
    {
        this.LocalConfig = localConfig;
        this.LocalModuleConfig = localModuleConfig;
        this.Channels = channels;
        this.MyNodeInfo = myNodeInfo;
        this.Nodes = nodes;
    }

    public DeviceStateContainer()
    {
        this.LocalConfig = new LocalConfig();
        this.LocalModuleConfig = new LocalModuleConfig();
        this.Channels = new List<Channel>();
        this.MyNodeInfo = new MyNodeInfo();
        this.Nodes = new List<NodeInfo>();
    }

    private void SetConfig(Config.PayloadVariantOneofCase variant, Config config)
    {
        var variantName = variant.ToString();
        var variantValue = typeof(Config).GetProperty(variantName)?.GetValue(config);
        typeof(LocalConfig).GetProperty(variantName)?.SetValue(this.LocalConfig, variantValue);
    }

    private void SetModuleConfig(ModuleConfig.PayloadVariantOneofCase variant, ModuleConfig config)
    {
        var variantName = variant.ToString();
        var variantValue = typeof(ModuleConfig).GetProperty(variantName)?.GetValue(config);
        typeof(LocalModuleConfig).GetProperty(variantName)?.SetValue(this.LocalModuleConfig, variantValue);
    }

    public void AddFromRadio(FromRadio fromRadio)
    {
        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Config)
            SetConfig(fromRadio.Config.PayloadVariantCase, fromRadio.Config);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.ModuleConfig)
            SetModuleConfig(fromRadio.ModuleConfig.PayloadVariantCase, fromRadio.ModuleConfig);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Channel)
            this.Channels.Add(fromRadio.Channel);

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.MyInfo)
            this.MyNodeInfo = fromRadio.MyInfo;

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.NodeInfo)
            this.Nodes.Add(fromRadio.NodeInfo);
    }

    public uint GetAdminChannelIndex()
    {
        return (uint)(this.Channels.FirstOrDefault(p => p.Role == Channel.Types.Role.Secondary &&
            string.Equals(p.Settings.Name, "admin", StringComparison.OrdinalIgnoreCase))?.Index ?? 0);
    }

    public uint GetHopLimitOrDefault()
    {
        return (uint)(this.LocalConfig.Lora.HopLimit > 0 ? this.LocalConfig.Lora.HopLimit : 3);
    }

    public string GetNodeDisplayName(uint nodeNum)
    {
        var node = this.Nodes.Find(n => n.Num == nodeNum);
        if (node == null)
            return nodeNum.ToString();

        return $"{node.User.LongName} ({node.User.ShortName}) - {node.Num}";
    }
}
