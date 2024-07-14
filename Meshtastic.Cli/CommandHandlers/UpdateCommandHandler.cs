using Meshtastic.Cli.Mappings;
using Meshtastic.Cli.Utilities;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Protobufs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Meshtastic.Cli.CommandHandlers;

[ExcludeFromCodeCoverage(Justification = "Requires interaction")]
public class UpdateCommandHandler : DeviceCommandHandler
{
    public UpdateCommandHandler(FirmwarePackageService firmwarePackageService, ReleaseZipService releaseZipService, DeviceConnectionContext context, CommandContext commandContext)
        : base(context, commandContext)
    {
        this.firmwarePackageService = firmwarePackageService;
        this.releaseZipService = releaseZipService;
    }

    private DeviceStateContainer? deviceStateContainer;
    private readonly FirmwarePackageService firmwarePackageService;
    private readonly ReleaseZipService releaseZipService;

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = new ToRadioMessageFactory().CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        await StartInteractiveFlashUpdate(deviceStateContainer!.Metadata.HwModel);
        return container;
    }

    private async Task StartInteractiveFlashUpdate(HardwareModel hardwareModel)
    {
        var releaseOptions = await firmwarePackageService.GetFirmwareReleases();
        var selection = GetFirmwareSelection(releaseOptions, "Which release or pull-request?");
        var release = AnsiConsole.Prompt(selection);
        var memoryStream = await firmwarePackageService.DownloadRelease(release);
        var filePath = await releaseZipService.ExtractUpdateBinary(memoryStream, hardwareModel);

        if (HardwareModelMappings.NrfHardwareModels.Contains(hardwareModel))
        {
            var _ = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .AddChoices(new[] { "Yes" }) 
                .Title("Have you double-pressed the RST button to enter DFU mode?"));
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
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Flashing", async (ctx) =>
            {
                AnsiConsole.WriteLine($"Copying uf2 file to {drive}");
                File.Copy(uf2Path, Path.Combine(drive, new System.IO.FileInfo(uf2Path).Name));
                await Task.Delay(2000);
                File.Delete(uf2Path);
                AnsiConsole.Write("Completed device update!");
            });
    }

    private static void EsptoolUpdate(string binPath, string port)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Flashing", (ctx) =>
            {
                var info = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                SetEsptoolProcessInfo(info, binPath, port);
                using var process = Process.Start(info);
                process!.OutputDataReceived += (sender, args) => AnsiConsole.WriteLine(args.Data ?? String.Empty);
                process!.ErrorDataReceived += (sender, args) => AnsiConsole.WriteLine(args.Data ?? String.Empty);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();
                File.Delete(binPath);
                AnsiConsole.Write("Completed device update!");
            });
    }

    private static void SetEsptoolProcessInfo(ProcessStartInfo processStartInfo, string filePath, string port)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = $"cmd /C \"python -m esptool --baud 921600 write_flash 0x10000 {filePath} -p {port}\"";
        }
        else
        {
            processStartInfo.FileName = "esptool.py";
            processStartInfo.Arguments = $"--baud 921600 write_flash 0x10000 {filePath} -p {port}";
        }
    }

    private static string GetSelectedDrive()
    {
        return AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Which drive is the device?")
            .AddChoices(Directory.GetLogicalDrives()));
    }

    private static SelectionPrompt<FirmwarePackage> GetFirmwareSelection(FirmwarePackageOptions releases, string promptText)
    {
        var selection = new SelectionPrompt<FirmwarePackage>()
            .Title(promptText)
            .PageSize(20)
            .AddChoiceGroup(new FirmwarePackage("Stable", String.Empty), releases.releases.stable)
            .AddChoiceGroup(new FirmwarePackage("Alpha", String.Empty), releases.releases.alpha)
            .AddChoiceGroup(new FirmwarePackage("Pull-requests", String.Empty), releases
                .pullRequests
                .Select(pr => new FirmwarePackage(pr.title.EscapeMarkup(), pr.zip_url)));
        selection.Converter = r => r.title;
        return selection;
    }

    public override Task OnCompleted(FromRadio packet, DeviceStateContainer container)
    {
        deviceStateContainer = container;
        return Task.CompletedTask;
    }
}
