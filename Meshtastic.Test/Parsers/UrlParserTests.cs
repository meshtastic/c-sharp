using Meshtastic.Cli.Parsers;

namespace Meshtastic.Test.Parsers;

public class UrlParserTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Parse_Should_ReturnChannelSet_GivenValidMeshtasticUrl()
    {
        var url = "https://meshtastic.org/e/#CjMSIK4kboySjOqKWHn8dCR2Gp7L0syIGv1_sySCrbZxneV2GgtCZW5zRnVuTGFuZCgBMAEKKRIgarYveKnCBHSGrOMkzVVStMEElngYZQir38xCiDKkj6UaBWFkbWluCisSIExfvRlaRwJWHDEvQYaUjzwUeN4FvkI_6nJX9P0ByHCOGgdQYXJ0eU9uEgoIATgBQANIAVAe";
        var parser = new UrlParser(url);
        var result = parser.Parse();
        result.Settings[0].Name.Should().Be("BensFunLand");
    }

    [Test]
    public void Should_ThrowException_GivenNoMeshtasticUrl()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var action = () => new UrlParser(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Should_ThrowException_GivenEmptyMeshtasticUrl()
    {
        var action = () => new UrlParser(String.Empty);
        action.Should().Throw<ArgumentException>();
    }
}
