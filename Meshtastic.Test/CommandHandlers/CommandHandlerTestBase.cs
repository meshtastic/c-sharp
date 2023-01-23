using Meshtastic.Cli;
using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Test.CommandHandlers
{
    public class CommandHandlerTestBase
    {
        public Mock<ILogger> FakeLogger = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Consistency")]
        public DeviceConnectionContext ConnectionContext => new(null, "localhost");
        public CommandContext CommandContext => new(FakeLogger.Object, OutputFormat.PrettyConsole, null, false);

        protected void DebugLogsContain(string messagePart, Times? times = null)
        {
            FakeLogger.VerifyLog(l =>
            l.LogDebug(It.Is<string>(message => message.StartsWith(messagePart))), times ?? Times.Once());
        }
        protected void InformationLogsContain(string messagePart, Times? times = null)
        {
            FakeLogger.VerifyLog(l =>
            l.LogInformation(It.Is<string>(message => message.StartsWith(messagePart))), times ?? Times.Once());
        }

        protected void ReceivedWantConfigPayloads()
        {
            DebugLogsContain("Sent: { \"wantConfigId\":");
            DebugLogsContain("Received: { \"myInfo\": {");
            DebugLogsContain("Received: { \"nodeInfo\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"channel\": {", Times.Exactly(8));
            DebugLogsContain("Received: { \"config\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"moduleConfig\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"configCompleteId\":");
        }
    }
}