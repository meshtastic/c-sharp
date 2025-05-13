using System.Security.Cryptography.X509Certificates;
using Google.Protobuf;
using Meshtastic.Cli.Parsers;
using Meshtastic.Protobufs;

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
        var result = parser.ParseChannels();
        result.Settings[0].Name.Should().Be("BensFunLand");
    }

    [Test]
    public void Parse_Should_ReturnContact_GivenValidMeshtasticUrl()
    {
        var contact = new SharedContact
        {
            NodeNum = 123456,
            User = new User
            {
                Id = $"!{Convert.ToHexString(BitConverter.GetBytes(123456))}",
                LongName = "Benben",
                ShortName = "B",
                PublicKey = ByteString.CopyFrom(
                [
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                    0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                    0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
                ]),
            }
        };

        var serialized = contact.ToByteArray();
        var base64 = Convert.ToBase64String(serialized);
        base64 = base64.Replace("-", String.Empty).Replace('+', '-').Replace('/', '_');

        var url = $"https://meshtastic.org/v/#{base64}";
        var parser = new UrlParser(url);
        var result = parser.ParseContact();
        result.User.LongName.Should().Be("Benben");
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
