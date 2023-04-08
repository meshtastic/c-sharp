using Google.Protobuf;
using Meshtastic.Protobufs;
using System.Linq;

namespace Meshtastic.Data;

public class DeviceStateContainer
{
    public LocalConfig LocalConfig;
    public LocalModuleConfig LocalModuleConfig;
    public List<Channel> Channels;
    public MyNodeInfo MyNodeInfo;
    public List<NodeInfo> Nodes;
    public List<FromRadio> FromRadioMessageLog;
    public List<ToRadio> ToRadioMessageLog;
    public DeviceMetadata Metadata;

    public DeviceStateContainer()
    {
        this.LocalConfig = new LocalConfig();
        this.LocalModuleConfig = new LocalModuleConfig();
        this.Channels = new List<Channel>();
        this.MyNodeInfo = new MyNodeInfo();
        this.Nodes = new List<NodeInfo>();
        this.ToRadioMessageLog = new List<ToRadio>();
        this.FromRadioMessageLog = new List<FromRadio>();
        this.Metadata = new DeviceMetadata();
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
            this.Nodes = this.Nodes.Where(n => n.Num != fromRadio.NodeInfo.Num).Append(fromRadio.NodeInfo).ToList();

        if (fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Metadata)
            this.Metadata = fromRadio.Metadata;

        this.FromRadioMessageLog.Insert(0, fromRadio);
    }

    public void AddToRadio(ToRadio toRadio)
    {
        this.ToRadioMessageLog.Add(toRadio);
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

    public NodeInfo? GetDeviceNodeInfo()
    {
        return this.Nodes.Find(n => n.Num == this.MyNodeInfo.MyNodeNum);
    }

    public string GetNodeDisplayName(uint nodeNum, bool shortName = false, bool hideNodeNum = false)
    {
        var node = this.Nodes.Find(n => n.Num == nodeNum);
        if (node == null)
            return nodeNum.ToString();

        if (shortName)
            return node?.User?.ShortName ?? String.Empty;

        if (hideNodeNum)
            return $"{node?.User?.LongName} ({node?.User?.ShortName})";

        return $"{node?.User?.LongName} ({node?.User?.ShortName}) - {node?.Num}";
    }

    public string GetChannelUrl()
    {
        var channelSet = new ChannelSet()
        {
            LoraConfig = this.LocalConfig.Lora
        };
        this.Channels.ForEach(channel =>
        {
            channelSet.Settings.Add(channel.Settings);
        });
        var serialized = channelSet.ToByteArray();
        var base64 = Convert.ToBase64String(serialized);
        base64 = base64.Replace("-", String.Empty).Replace('+', '-').Replace('/', '_');
        return $"https://meshtastic.org/e/#{base64}".TrimEnd('=');
    }
}
