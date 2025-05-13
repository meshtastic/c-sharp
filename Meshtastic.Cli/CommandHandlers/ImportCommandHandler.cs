using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Extensions.Logging;
using Meshtastic.Cli.Parsers;
using Meshtastic.Cli.Extensions;

namespace Meshtastic.Cli.CommandHandlers;

public class ImportCommandHandler : DeviceCommandHandler
{
    private readonly IDeserializer yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private readonly string inputFile;

    public ImportCommandHandler(string file, DeviceConnectionContext context, CommandContext commandContext) 
        : base(context, commandContext) 
    { 
        this.inputFile = file ?? Path.Combine(Directory.GetCurrentDirectory(), "export.yml");
    }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        if (!File.Exists(inputFile))
        {
            Logger.LogError("Could not find input file '{0}'", inputFile);
            return;
        }
        Logger.LogInformation($"Importing profile from: {inputFile}");
        var contents = await File.ReadAllTextAsync(inputFile);
        var deviceProfile = yamlDeserializer.Deserialize<DeviceProfile>(contents);

        var adminMessageFactory = new AdminMessageFactory(container, Destination);
        await BeginEditSettings(adminMessageFactory);

        if (deviceProfile.HasLongName) 
        {
            Logger.LogInformation("Setting long / short name...");
            var user = new User()
            {
                LongName = deviceProfile.LongName,
                ShortName = deviceProfile.ShortName
            };
            var setOwner = adminMessageFactory.CreateSetOwnerMessage(user);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setOwner), AnyResponseReceived);
        }
        
        if (deviceProfile.HasChannelUrl) 
            await SetChannels(adminMessageFactory, deviceProfile.ChannelUrl);
        else
            Logger.LogDebug("No Channels specified. Skipping...");
        
        if (deviceProfile.Config != null)
            await SetConfigs(container, deviceProfile, adminMessageFactory);
        else 
            Logger.LogDebug("No Config profile specified. Skipping...");

        if (deviceProfile.ModuleConfig != null)
            await SetModuleConfigs(container, deviceProfile, adminMessageFactory);
        else
            Logger.LogDebug("No ModuleConfig profile specified. Skipping...");


        await CommitEditSettings(adminMessageFactory);
    }

    private async Task SetConfigs(DeviceStateContainer container, DeviceProfile deviceProfile, AdminMessageFactory adminMessageFactory)
    {
        foreach(var section in deviceProfile.Config.GetProperties())
        {
            var instance = section.GetValue(deviceProfile.Config);
            if (instance == null) {
                Logger.LogDebug($"No {section.Name} profile specified. Skipping...");
                continue;
            }
            Logger.LogInformation($"Sending {section.Name} config to device...");
            //container.LocalConfig.Bluetooth.MergeFrom()
            var adminMessage = adminMessageFactory.CreateSetConfigMessage(instance);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        }
    }

    private async Task SetModuleConfigs(DeviceStateContainer container, DeviceProfile deviceProfile, AdminMessageFactory adminMessageFactory)
    {
        foreach(var section in deviceProfile.ModuleConfig.GetProperties())
        {
            var instance = section.GetValue(deviceProfile.ModuleConfig);
            if (instance == null) {
                Logger.LogDebug($"No {section.Name} profile specified. Skipping...");
                continue;
            }
            Logger.LogInformation($"Sending {section.Name} config to device...");
            var adminMessage = adminMessageFactory.CreateSetModuleConfigMessage(instance);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(adminMessage), AnyResponseReceived);
        }
    }

    private async Task SetChannels(AdminMessageFactory adminMessageFactory, string url)
    {
        var urlParser = new UrlParser(url);
        var channelSet = urlParser.ParseChannels();
        int index = 0;
        foreach (var setting in channelSet.Settings)
        {
            var channel = new Channel
            {
                Index = index,
                Role = index == 0 ? Channel.Types.Role.Primary : Channel.Types.Role.Secondary,
                Settings = setting
            };
            Logger.LogInformation($"Sending Channel {index} to device...");
            var setChannel = adminMessageFactory.CreateSetChannelMessage(channel);
            await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setChannel), AnyResponseReceived);
            index++;
        }
        Logger.LogInformation("Sending LoRA config to device...");

        var setLoraConfig = adminMessageFactory.CreateSetConfigMessage(channelSet.LoraConfig);
        await Connection.WriteToRadio(ToRadioMessageFactory.CreateMeshPacketMessage(setLoraConfig), AnyResponseReceived);
    }
}
