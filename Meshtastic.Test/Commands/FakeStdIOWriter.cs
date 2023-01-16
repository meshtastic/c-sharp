using System.CommandLine.IO;

namespace Meshtastic.Test.Commands
{
    public class FakeStdIOWriter : IStandardStreamWriter
    {
        public string Output = String.Empty;
        public void Write(string? value)
        {
            Output += value ?? String.Empty;
        }
    }
}
