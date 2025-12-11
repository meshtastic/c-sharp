using Meshtastic.Cli.Utilities;
using System.CommandLine;

namespace Meshtastic.Test.Utilities;

public class ArgumentParsersTests
{
    [TestCase("!DEADBEEF", 3735928559u, false)]
    [TestCase("!deadbeef", 3735928559u, false)]
    [TestCase("!d", null, true)]
    [TestCase("0xDEADBEEF", 3735928559u, false)]
    [TestCase("0xdeadbeef", 3735928559u, false)]
    [TestCase("0xd", null, true)]
    [TestCase("4294967295", uint.MaxValue, false)]
    [TestCase("42949672950", null, true)]
    [TestCase("429496729a", null, true)]
    public async Task NodeIdParser_ParseHex(string valueForParse, uint? expected, bool withError)
    {
        var opt = new Option<uint?>("--dest", 
            description: "Destination node address for command",
            parseArgument: ArgumentParsers.NodeIdParser);

        var result = opt.Parse(["--dest", valueForParse]);
        if (!withError)
        {
            result.Errors.Should().HaveCount(0);
            result.GetValueForOption(opt).Should().Be(expected);
        }
        else
        {
            result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
        }
    }
}
