using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Meshtastic.Test.Commands
{
    public class CommandTestBase
    {
        protected Option<string> portOption = new("--port", "");
        protected Option<string> hostOption = new("--host", "");
        protected Option<OutputFormat> outputOption = new("--output", "");
        protected Option<LogLevel> logLevelOption = new("--log", "");
        protected Option<uint?> destOption = new("--dest", "");
        protected Option<bool> selectDestOption = new("--select-dest", "");

        protected RootCommand GetRootCommand()
        {
            var root = new RootCommand("Meshtastic.Cli");
            root.AddGlobalOption(portOption);
            root.AddGlobalOption(hostOption);
            root.AddGlobalOption(outputOption);
            root.AddGlobalOption(logLevelOption);
            root.AddGlobalOption(destOption);
            root.AddGlobalOption(selectDestOption);
            return root;
        }

        public FakeStdIOWriter Error { get; private set; }
        public FakeStdIOWriter Out { get; private set; }
        public FakeConsole Console { get; private set; }

        [SetUp]
        public void BaseSetup()
        {
            Error = new FakeStdIOWriter();
            Out = new FakeStdIOWriter();
            Console = new FakeConsole(Error, Out);
        }
    }
}