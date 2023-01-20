using Octokit;

namespace Meshtastic.Cli.Utilities;

public record FirmwareRelease(string Name, string DownloadUrl);

public class GithubService
{
    public GithubService()
    {
    }

    public async Task<IEnumerable<FirmwareRelease>> GetLatest5Releases()
    {
        var client = new GitHubClient(new ProductHeaderValue("meshtastic"));
        var releases = await client.Repository.Release.GetAll("meshtastic", "firmware");
        var curatedReleases = releases.Where(r => r.Name.Contains("2.0") && !r.Name.Contains("Revoked")).Take(5).ToList();
        return curatedReleases.Select(r =>
        {
            return new FirmwareRelease(
                r.Name.Replace("Meshtastic Firmware ", String.Empty),
                r.Assets.First(a => a.Name.StartsWith("firmware")).BrowserDownloadUrl);
        });
    }

    public async Task<MemoryStream> DownloadRelease(FirmwareRelease firmwareRelease)
    {
        var memoryStream = new MemoryStream();
        using var httpClient = new HttpClient();
        var stream = await httpClient.GetStreamAsync(firmwareRelease.DownloadUrl);
        await stream.CopyToAsync(memoryStream);
        return memoryStream;
    }
}
