using System.CommandLine;
using System.CommandLine.IO;

namespace Meshtastic.Test.Commands
{
    public class FakeConsole : IConsole
    {
        public FakeConsole(FakeStdIOWriter errorStd, FakeStdIOWriter outputStd)
        {
            ErrorStd = errorStd;
            OutputStd = outputStd;
        }

        public bool IsOutputRedirected => true;

        public bool IsErrorRedirected => true;

        public bool IsInputRedirected => true;

        public FakeStdIOWriter ErrorStd { get; }
        public FakeStdIOWriter OutputStd { get; }

        public IStandardStreamWriter Out => ErrorStd;

        public IStandardStreamWriter Error => OutputStd;
    }
}
