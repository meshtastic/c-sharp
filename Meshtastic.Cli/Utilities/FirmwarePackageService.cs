using System.Net.Http.Json;

namespace Meshtastic.Cli.Utilities;

public record FirmwareReleaseOptions(IEnumerable<FirmwarePackage> stable, IEnumerable<FirmwarePackage> alpha);
public record FirmwarePackageOptions(FirmwareReleaseOptions releases, IEnumerable<FirmwarePackage> pullRequests);
public record FirmwarePackage(string title, string zip_url);

public class FirmwarePackageService
{
    public FirmwarePackageService()
    {
    }

    public async Task<FirmwarePackageOptions> GetFirmwareReleases()
    {
        var client = new HttpClient();
#pragma warning disable CS8603 // Possible null reference return.
        return await client.GetFromJsonAsync<FirmwarePackageOptions>("https://api.meshtastic.org/github/firmware/list");
#pragma warning restore CS8603 // Possible null reference return.
    }

    public async Task<MemoryStream> DownloadRelease(FirmwarePackage firmwareRelease)
    {
        var memoryStream = new MemoryStream();
        using var httpClient = new HttpClient();
        var stream = await httpClient.GetStreamAsync(firmwareRelease.zip_url);
        await stream.CopyToAsync(memoryStream);
        return memoryStream;
    }
}
