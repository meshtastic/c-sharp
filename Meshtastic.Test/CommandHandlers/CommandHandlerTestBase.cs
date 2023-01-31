using Meshtastic.Cli;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Test.CommandHandlers
{
    public class CommandHandlerTestBase
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mock<ILogger> FakeLogger;
        public DeviceConnectionContext ConnectionContext;
        public CommandContext CommandContext;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
        protected void ErrorLogsContain(string messagePart, Times? times = null)
        {
            FakeLogger.VerifyLog(l =>
            l.LogError(It.Is<string>(message => message.StartsWith(messagePart))), times ?? Times.Once());
        }

        protected void ReceivedWantConfigPayloads()
        {
            DebugLogsContain("Sent: { \"wantConfigId\":");
            DebugLogsContain("Received: { \"myInfo\": {");
            DebugLogsContain("Received: { \"nodeInfo\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"channel\": {", Times.AtLeast(7));
            DebugLogsContain("Received: { \"config\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"moduleConfig\": {", Times.AtLeastOnce());
            DebugLogsContain("Received: { \"configCompleteId\":");
        }
    }
}