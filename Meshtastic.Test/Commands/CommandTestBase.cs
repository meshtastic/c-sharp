using Meshtastic.Cli.Enums;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Meshtastic.Test.Commands
{
    public class CommandTestBase
    {
        protected Option<string> portOption = new Option<string>("--port", "");
        protected Option<string> hostOption = new Option<string>("--host", "");
        protected Option<OutputFormat> outputOption = new Option<OutputFormat>("--output", "");
        protected Option<LogLevel> logLevelOption = new("--log", "");
        protected Option<uint?> destOption = new Option<uint?>("--dest", "");
        protected Option<bool> selectDestOption = new Option<bool>("--select-dest", "");

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
        public void Setup()
        {
            Error = new FakeStdIOWriter();
            Out = new FakeStdIOWriter();
            Console = new FakeConsole(Error, Out);
        }
    }
}