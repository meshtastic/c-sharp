using System.IO.Ports;

namespace Meshtastic.Handlers
{
    public class NoProtoHandler : ICommandHandler<string> 
    {
        public static async Task Handle(string port) 
        {
            var serialPort = new SerialPort(port, Resources.DEFAULT_BAUD_RATE);
            try
            {
                serialPort.Open();
                while (serialPort.IsOpen) 
                {
                    if (serialPort.BytesToRead > 0) {
                        Console.Write(serialPort.ReadExisting());
                    }
                    await Task.Delay(10);
                }
                Console.WriteLine("Serial disconnected");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            await Task.FromResult(0);
        }
    }
}
