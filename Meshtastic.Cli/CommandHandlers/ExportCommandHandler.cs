using Meshtastic.Cli.Extensions;
using Meshtastic.Cli.Serialization;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Meshtastic.Cli.CommandHandlers;

public class ExportCommandHandler : DeviceCommandHandler
{
    private readonly ISerializer yamlSerializer = new SerializerBuilder()
        .IgnoreFields()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeInspector(inner => new FilteredTypeInspector(inner))
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)
        .Build();

    private readonly string outputFile;

    public ExportCommandHandler(string file, DeviceConnectionContext context, CommandContext commandContext) : base(context, commandContext)
    {
        this.outputFile = file ?? Path.Combine(Directory.GetCurrentDirectory(), "export.yml");
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
        var channels = container.Channels.Where(c => c.Role != Channel.Types.Role.Disabled).Select(c => $"{c.Index} ({(c.Role == Channel.Types.Role.Primary ? "PRIMARY" : c.Settings.Name)})");
        var configs = container.LocalConfig.GetProperties().Select(p => p.Name);
        var moduleConfigs = container.LocalModuleConfig.GetProperties().Select(p => p.Name);

        var prompt = new MultiSelectionPrompt<string>()
            .Required(true)
            .PageSize(100)
            .Title("What would you like to export?")
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle selection and [green]<enter>[/] to accept)[/]")
            .AddChoices("Name")
            .AddChoiceGroup("Channels", channels)
            .AddChoiceGroup("Config", configs)
            .AddChoiceGroup("Module Config", moduleConfigs)
            .Select("Name")
            .Select("Channels")
            .Select("Config")
            .Select("Module Config");

        foreach(var option in channels.Union(configs).Union(moduleConfigs))
        {
            prompt = prompt.Select(option);
        }

        var selection = AnsiConsole.Prompt(prompt);

        var deviceProfile = new DeviceProfile();

        if (container.LocalConfig.GetProperties().Any(p => selection.Contains(p.Name)))
        {
            var localConfig = new LocalConfig();
            foreach(var selectedSection in container.LocalConfig.GetProperties().Where(p => selection.Contains(p.Name)))
            {
                var property = typeof(LocalConfig).FindPropertyByName(selectedSection.Name)!;
                var val = property.GetValue(container.LocalConfig);
                property.SetValue(localConfig, val);
            }
            deviceProfile.Config = localConfig;
        }

        if (container.LocalModuleConfig.GetProperties().Any(p => selection.Contains(p.Name)))
        {
            var localModuleConfig = new LocalModuleConfig();
            foreach (var selectedSection in container.LocalModuleConfig.GetProperties().Where(p => selection.Contains(p.Name)))
            {
                var property = typeof(LocalModuleConfig).FindPropertyByName(selectedSection.Name)!;
                var val = property.GetValue(container.LocalModuleConfig);
                property.SetValue(localModuleConfig, val);
            }
            deviceProfile.ModuleConfig = localModuleConfig;
        } 

        var me = container.GetDeviceNodeInfo();
        if (selection.Contains("Name") && me?.User != null)
        {
            deviceProfile.LongName = me.User.LongName;
            deviceProfile.ShortName = me.User.ShortName;
        }
        var selectedChannelIndexes = selection.Select(s => s.First())
                .Where(s => Char.IsDigit(s))
                .Select(s => Int32.Parse(s.ToString()))
                .ToArray();

        if (selectedChannelIndexes.Any())
            deviceProfile.ChannelUrl = container.GetChannelUrl(selectedChannelIndexes);

        var serialized = yamlSerializer.Serialize(deviceProfile);

        AnsiConsole.WriteLine(serialized);

        await File.WriteAllTextAsync(outputFile, serialized);
        Logger.LogInformation($"Exported profile written to: {outputFile}");
    }
}
