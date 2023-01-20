using Meshtastic.Cli.Mappings;
using Meshtastic.Cli.Utilities;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static SimpleExec.Command;

namespace Meshtastic.Cli.CommandHandlers;

[ExcludeFromCodeCoverage(Justification = "Requires interaction")]
public class UpdateCommandHandler : DeviceCommandHandler
{
    public UpdateCommandHandler(GithubService githubService, ReleaseZipService releaseZipService, DeviceConnectionContext context, CommandContext commandContext)
        : base(context, commandContext)
    {
        this.githubService = githubService;
        this.releaseZipService = releaseZipService;
    }

    private static readonly string PullRequest = "Pull-request";
    private static readonly string FirmwareRelease = "Firmware Release";

    private DeviceStateContainer? deviceStateContainer;
    private readonly GithubService githubService;
    private readonly ReleaseZipService releaseZipService;

    public async Task Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        var hardwareModel = deviceStateContainer!.Nodes.First(n => n.Num == deviceStateContainer.MyNodeInfo.MyNodeNum).User.HwModel;
        await StartInteractiveFlash(hardwareModel);
    }

    private async Task StartInteractiveFlash(HardwareModel hardwareModel)
    {
        var prOrRelease = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Flash update from pull-request or firmware release?")
            .AddChoices(new[] { FirmwareRelease, PullRequest }));

        if (prOrRelease == PullRequest)
            throw new NotImplementedException();
        else
            await StartInteractiveFlashUpdate(hardwareModel);
    }

    private async Task StartInteractiveFlashUpdate(HardwareModel hardwareModel)
    {
        var releases = await githubService.GetLatest5Releases();
        var selection = GetFirmwareSelection(releases, "Which release version?");
        var release = AnsiConsole.Prompt(selection);
        var memoryStream = await githubService.DownloadRelease(release);
        var filePath = await releaseZipService.ExtractUpdateBinary(memoryStream, hardwareModel);

        if (HardwareModelMappings.NrfHardwareModels.Contains(hardwareModel))
        {
            var drive = GetSelectedDrive();
            await Uf2Update(drive, filePath);
        }
        else
        {
            EsptoolUpdate(filePath, ConnectionContext.Port!);
        }
    }

    private static async Task Uf2Update(string drive, string uf2Path)
    {
        AnsiConsole.WriteLine($"Copying uf2 file to {drive}");
        await RunAsync("cp", uf2Path);
        AnsiConsole.WriteLine($"Copying complete");
    }

    private static void EsptoolUpdate(string filePath, string port)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Flashing", (ctx) =>
        {
            var info = new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = $"cmd /C \"python -m esptool --baud 921600 write_flash 0x10000 {filePath} -p {port}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using var process = Process.Start(info);
            process!.OutputDataReceived += (sender, args) => AnsiConsole.WriteLine(args.Data ?? String.Empty);
            process!.ErrorDataReceived += (sender, args) => AnsiConsole.WriteLine(args.Data ?? String.Empty);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
            AnsiConsole.Write("Completed device update!");
        });
    }

    private static string GetSelectedDrive()
    {
        return AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Which drive?")
            .AddChoices(Directory.GetLogicalDrives()));
    }

    private static SelectionPrompt<FirmwareRelease> GetFirmwareSelection(IEnumerable<FirmwareRelease> releases, string promptText)
    {
        var selection = new SelectionPrompt<FirmwareRelease>()
            .Title(promptText)
            .AddChoices(releases);
        selection.Converter = r => r.Name;
        return selection;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        deviceStateContainer = container;
        return Task.CompletedTask;
    }
}
