using Meshtastic.Cli.Utilities;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Utilities
{
    [TestFixture]
    public class ReleaseZipServiceTests
    {
        [Test]
        public async Task ExtractBinaries_Should_DownloadUf2_ForLatestRakRelease()
        {
            var service = new ReleaseZipService();
            var github = new GithubService();
            var releases = await github.GetLatest5Releases();
            var memoryStream = await github.DownloadRelease(releases.First());
            var path = await service.ExtractUpdateBinary(memoryStream, HardwareModel.Rak4631);
            path.Should().EndWith(".uf2");
        }

        [Test]
        public async Task ExtractBinaries_Should_DownloadUpdateBin_ForLatestEsp32Release()
        {
            var service = new ReleaseZipService();
            var github = new GithubService();
            var releases = await github.GetLatest5Releases();
            var memoryStream = await github.DownloadRelease(releases.First());
            var path = await service.ExtractUpdateBinary(memoryStream, HardwareModel.Tbeam);
            path.Should().EndWith("update.bin");
        }
    }
}
