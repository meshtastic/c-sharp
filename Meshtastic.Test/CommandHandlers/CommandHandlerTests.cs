using Meshtastic.Cli.Binders;
using Meshtastic.Cli.CommandHandlers;
using Meshtastic.Cli.Enums;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.CommandHandlers;

[TestFixture]
[Category(TestCategories.SimulatedDeviceTests)]
[NonParallelizable]
[Timeout(10000)]
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
    [Retry(3)]
    public async Task InfoCommandHandler_Should_ReceiveWantConfigPayloads()
    {
        var handler = new InfoCommandHandler(ConnectionContext, CommandContext);
        await handler.Handle();

        ReceivedWantConfigPayloads();
    }

    [Test]
    [Retry(3)]
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
    [Retry(3)]
    public async Task GetCommandHandler_Should_ReceiveWantConfigPayloads()
    {
        var settings = new List<string>() { "power.ls_secs", "mqtt.address" };
        var handler = new GetCommandHandler(settings, ConnectionContext, CommandContext);
        await handler.Handle();

        ReceivedWantConfigPayloads();
    }

    [Test]
    [Retry(3)]
    public void GetCommandHandler_Should_RejectBadSettings()
    {
        var settings = new List<string>() { "butt.farts" };
        var handler = new GetCommandHandler(settings, ConnectionContext, CommandContext);
        handler.ParsedSettings.Should().BeNull();
    }

    [Test]
    [Retry(3)]
    public async Task SetCommandHandler_Should_SetValues()
    {
        var settings = new List<string>() { "display.screen_on_secs=123456" };
        var handler = new SetCommandHandler(settings, ConnectionContext, CommandContext);
        await handler.Handle();

        var container = await new InfoCommandHandler(ConnectionContext, CommandContext).Handle();
        container.LocalConfig.Display.ScreenOnSecs.Should().Be(123456);
    }

    [Test]
    [Retry(3)]
    public void SetCommandHandler_Should_RejectBadSettings()
    {
        var settings = new List<string>() { "butt.farts=2" };
        var handler = new SetCommandHandler(settings, ConnectionContext, CommandContext);
        handler.ParsedSettings.Should().BeNull();
    }

    [Test]
    [Retry(3)]
    public async Task FixedPositionCommandHandler_Should_Acknowledge()
    {
        var handler = new FixedPositionCommandHandler(34.00m, -92.000m, 123, ConnectionContext, CommandContext);
        var container = await handler.Handle();
        InformationLogsContain("Sending position to device");
        InformationLogsContain("Setting Position.FixedPosition to True");
        var routingPacket = container.FromRadioMessageLog.First(fromRadio => fromRadio.GetMessage<Routing>() != null);
        routingPacket.GetMessage<Routing>()!.ErrorReason.Should().Be(Routing.Types.Error.None);
    }

    [Test]
    [Retry(3)]
    public async Task ChannelCommandHandler_Should_SavePrimaryChannel()
    {
        var channelSettings = new ChannelOperationSettings(ChannelOperation.Save, 0, "Test", Channel.Types.Role.Primary, "random", false, false);
        var handler = new ChannelCommandHandler(channelSettings, ConnectionContext, CommandContext);
        var container = await handler.Handle();
        InformationLogsContain("Writing channel");
        var routingPacket = container.FromRadioMessageLog.First(fromRadio => fromRadio.GetMessage<Routing>() != null);
        routingPacket.GetMessage<Routing>()!.ErrorReason.Should().Be(Routing.Types.Error.None);
        var adminMessages = container.ToRadioMessageLog.Where(toRadio => toRadio?.Packet?.Decoded.Portnum == PortNum.AdminApp);
        adminMessages.Should().Contain(adminMessage =>
            AdminMessage.Parser.ParseFrom(adminMessage.Packet.Decoded.Payload).PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.SetChannel);
    }

    [Test]
    [Retry(3)]
    public async Task ChannelCommandHandler_Should_SavePrimaryChannelWithNoPsk()
    {
        var channelSettings = new ChannelOperationSettings(ChannelOperation.Save, 0, "Test", Channel.Types.Role.Primary, "none", false, false);
        var handler = new ChannelCommandHandler(channelSettings, ConnectionContext, CommandContext);
        var container = await handler.Handle();
        InformationLogsContain("Writing channel");
        var routingPacket = container.FromRadioMessageLog.First(fromRadio => fromRadio.GetMessage<Routing>() != null);
        routingPacket.GetMessage<Routing>()!.ErrorReason.Should().Be(Routing.Types.Error.None);
        var adminMessages = container.ToRadioMessageLog.Where(toRadio => toRadio?.Packet?.Decoded.Portnum == PortNum.AdminApp);
        adminMessages.Should().Contain(adminMessage =>
            AdminMessage.Parser.ParseFrom(adminMessage.Packet.Decoded.Payload).PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.SetChannel);
    }

    [Test]
    [Retry(3)]
    public async Task ChannelCommandHandler_Should_SavePrimaryChannelWithPsk()
    {
        var channelSettings = new ChannelOperationSettings(ChannelOperation.Save, 0, "Test", Channel.Types.Role.Primary, "0x1a1a1a1a2b2b2b2b1a1a1a1a2b2b2b2b1a1a1a1a2b2b2b2b1a1a1a1a2b2b2b2b", false, false);
        var handler = new ChannelCommandHandler(channelSettings, ConnectionContext, CommandContext);
        var container = await handler.Handle();
        InformationLogsContain("Writing channel");
        var routingPacket = container.FromRadioMessageLog.First(fromRadio => fromRadio.GetMessage<Routing>() != null);
        routingPacket.GetMessage<Routing>()!.ErrorReason.Should().Be(Routing.Types.Error.None);
        var adminMessages = container.ToRadioMessageLog.Where(toRadio => toRadio?.Packet?.Decoded.Portnum == PortNum.AdminApp);
        adminMessages.Should().Contain(adminMessage =>
            AdminMessage.Parser.ParseFrom(adminMessage.Packet.Decoded.Payload).PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.SetChannel);
    }

    [Test]
    [Retry(3)]
    public async Task ChannelCommandHandler_Should_AllowEnableOfSecondary()
    {
        var channelSettings = new ChannelOperationSettings(ChannelOperation.Enable, 2, null, null, null, null, null);
        var handler = new ChannelCommandHandler(channelSettings, ConnectionContext, CommandContext);
        var container = await handler.Handle();
        InformationLogsContain("Writing channel");
        var routingPacket = container.FromRadioMessageLog.First(fromRadio => fromRadio.GetMessage<Routing>() != null);
        routingPacket.GetMessage<Routing>()!.ErrorReason.Should().Be(Routing.Types.Error.None);
        var adminMessages = container.ToRadioMessageLog.Where(toRadio => toRadio?.Packet?.Decoded.Portnum == PortNum.AdminApp);
        adminMessages.Should().Contain(adminMessage =>
            AdminMessage.Parser.ParseFrom(adminMessage.Packet.Decoded.Payload).PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.SetChannel);
    }

    [Test]
    [Retry(3)]
    public async Task ChannelCommandHandler_Should_ThrowExceptionForOutOfRangeIndex()
    {
        var channelSettings = new ChannelOperationSettings(ChannelOperation.Save, 10, null, null, null, null, null);
        var handler = new ChannelCommandHandler(channelSettings, ConnectionContext, CommandContext);
        var action = () => handler.Handle();
        await action.Should().ThrowAsync<IndexOutOfRangeException>();
    }
}