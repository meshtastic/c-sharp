using Google.Protobuf;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli.CommandHandlers;

public class FileCommandHandler : DeviceCommandHandler
{
    private readonly string path;
    private readonly MemoryStream memoryStream = new();


    public FileCommandHandler(string path, DeviceConnectionContext context, CommandContext commandContext) :
        base(context, commandContext)
    {
        this.path = path;
    }

    public async Task<DeviceStateContainer> Handle()
    {
        var wantConfig = ToRadioMessageFactory.CreateWantConfigMessage();
        var container = await Connection.WriteToRadio(wantConfig, CompleteOnConfigReceived);
        Connection.Disconnect();
        return container;
    }

    public override async Task OnCompleted(FromRadio _, DeviceStateContainer container)
    {
        var fileRequest = ToRadioMessageFactory.CreateXmodemPacketMessage();
        fileRequest.XmodemPacket.Buffer = ByteString.CopyFromUtf8(path);
        await Connection.WriteToRadio(fileRequest, async (fromRadio, container) =>
        {
            var xmodem = fromRadio.GetMessage<XModem>();
            if (xmodem == null)
                return false;

            var bufferArray = xmodem.Buffer.ToArray();
            await memoryStream.WriteAsync(bufferArray);

            if (xmodem.Control == XModem.Types.Control.Soh)
            {
                await Task.Delay(100);
                var ack = ToRadioMessageFactory.CreateXmodemPacketMessage(XModem.Types.Control.Ack);
                await Connection.WriteToRadio(ack);
            }
            else if (xmodem?.Control == XModem.Types.Control.Eot)
            {
                var fileName = Path.GetFileName(this.path);
                Logger.LogInformation("Retrieved file contents");
                Logger.LogInformation($"Writing to {fileName}");
                
                File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, fileName), memoryStream.ToArray());
                return true;
            }
            return false;
        });
    }
}
