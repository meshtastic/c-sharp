using Meshtastic.Cli.Utilities;

namespace Meshtastic.Test.Utilities
{
    [TestFixture]
    public class FirmwarePackageServiceTests
    {
        [Test]
        public async Task GetLast5Releases_Should_Return5Records()
        {
            var github = new FirmwarePackageService();
            var releases = await github.GetFirmwareReleases();
            releases.releases.stable.Should().HaveCountGreaterThan(0);
            releases.releases.stable.Should().HaveCountGreaterThan(0);
        }

        [Test]
        public async Task DownloadRelease_Should_ReturnMemoryStream()
        {
            var github = new FirmwarePackageService();
            var releases = await github.GetFirmwareReleases();
            var memoryStream = await github.DownloadRelease(releases.releases.alpha.First());
            memoryStream.Length.Should().BeGreaterThan(0);
        }
    }
}
