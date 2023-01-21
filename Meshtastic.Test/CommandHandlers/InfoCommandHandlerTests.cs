using Meshtastic.Cli.CommandHandlers;

namespace Meshtastic.Test.CommandHandlers;

[TestFixture]
[Category(TestCategories.SimulatedDeviceTests)]
public class InfoCommandHandlerTests : CommandHandlerTestBase
{
    private InfoCommandHandler commandHandler;

    [SetUp]
    public void Setup()
    {
        commandHandler = new InfoCommandHandler(ConnectionContext, CommandContext);
    }

    [Test]
    public async Task InfoCommandHandler_Should_ReceiveWantConfigPayloads()
    {
        await commandHandler.Handle();

        DebugLogsContain("Sent: { \"wantConfigId\":");
        DebugLogsContain("Received: { \"myInfo\": {");
        DebugLogsContain("Received: { \"nodeInfo\": {", Times.AtLeastOnce());
        DebugLogsContain("Received: { \"channel\": {", Times.Exactly(8));
        DebugLogsContain("Received: { \"config\": {", Times.AtLeastOnce());
        DebugLogsContain("Received: { \"moduleConfig\": {", Times.AtLeastOnce());
        DebugLogsContain("Received: { \"configCompleteId\":");
    }
}