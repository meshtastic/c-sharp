using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.CommandHandlers;

[TestFixture]
[Category(TestCategories.SimulatedDeviceTests)]
[NonParallelizable]
public class CommandHandlerTests : CommandHandlerTestBase
{
    [SetUp]
    public async Task Setup()
    {
        // Give the docker container some space between commands
        await Task.Delay(3000);

        FakeLogger = new();
        ConnectionContext = new(null, "localhost");
        CommandContext = new(FakeLogger.Object, OutputFormat.PrettyConsole, null, false);
    }

    [Test]
    public async Task InfoCommandHandler_Should_ReceiveWantConfigPayloads()
    {
        var handler = new InfoCommandHandler(ConnectionContext, CommandContext);
        await handler.Handle();

        ReceivedWantConfigPayloads();
    }

    [Test]
    public async Task MetadataCommandHandler_Should_ReceiveMetadataResponse()
    {
        var handler = new MetadataCommandHandler(ConnectionContext, CommandContext);
        var container = await handler.Handle();

        ReceivedWantConfigPayloads();
        InformationLogsContain("Getting device metadata");
        container.FromRadioMessageLog.Should()
            .Contain(fromRadio => fromRadio.GetMessage<AdminMessage>() != null && fromRadio.GetMessage<AdminMessage>()!.GetDeviceMetadataResponse != null);
    }
}