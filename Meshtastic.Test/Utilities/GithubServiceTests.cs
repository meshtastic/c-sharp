using Meshtastic.Cli.Utilities;
using NUnit.Framework;

namespace Meshtastic.Test.Utilities
{
    [TestFixture]
    public class GithubServiceTests
    {
        [Test]
        public async Task GetLast5Releases_Should_Return5Records()
        {
            var github = new GithubService();
            var releases = await github.GetLatest5Releases();
            releases.Should().HaveCount(5);
        }

        [Test]
        public async Task DownloadRelease_Should_ReturnMemoryStream()
        {
            var github = new GithubService();
            var releases = await github.GetLatest5Releases();
            var memoryStream = await github.DownloadRelease(releases.First());
            memoryStream.Length.Should().BeGreaterThan(0);
        }
    }
}
