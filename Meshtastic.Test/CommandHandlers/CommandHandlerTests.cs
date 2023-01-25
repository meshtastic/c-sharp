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
            .Contain(fromRadio => fromRadio.GetMessage<AdminMessage>() != null && 
                fromRadio.GetMessage<AdminMessage>()!.GetDeviceMetadataResponse != null);
    }

    [Test]
    public async Task GetCommandHandler_Should_ReceiveWantConfigPayloads()
    {
        var settings = new List<string>() { "power.ls_secs", "mqtt.address" };
        var handler = new GetCommandHandler(settings, ConnectionContext, CommandContext);
        await handler.Handle();

        ReceivedWantConfigPayloads();
    }

    [Test]
    public void GetCommandHandler_Should_RejectBadSettings()
    {
        var settings = new List<string>() { "butt.farts" };
        var handler = new GetCommandHandler(settings, ConnectionContext, CommandContext);
        handler.ParsedSettings.Should().BeNull();
    }


    [Test]
    public async Task SetCommandHandler_Should_SetValues()
    {
        var settings = new List<string>() { "display.screen_on_secs=123456" };
        var handler = new SetCommandHandler(settings, ConnectionContext, CommandContext);
        await handler.Handle();

        var container = await new InfoCommandHandler(ConnectionContext, CommandContext).Handle();
        container.LocalConfig.Display.ScreenOnSecs.Should().Be(123456);
    }

    [Test]
    public void SetCommandHandler_Should_RejectBadSettings()
    {
        var settings = new List<string>() { "butt.farts=2" };
        var handler = new SetCommandHandler(settings, ConnectionContext, CommandContext);
        handler.ParsedSettings.Should().BeNull();
    }
}