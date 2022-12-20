using Google.Protobuf;
using Meshtastic.Protobufs;
using System.Text;

namespace Meshtastic.Data
{
    public class FromDeviceMessage
    {
        private readonly FromRadio? fromRadio;
        private readonly AdminMessage? adminMessage;

        public FromDeviceMessage(byte[] bytes)
        {
            try
            {
                //Console.WriteLine($"Read: {Encoding.ASCII.GetString(bytes)}");
                fromRadio = FromRadio.Parser.ParseFrom(bytes);
                if (fromRadio?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.None)
                {
                    fromRadio = null;
                    var meshPacket = MeshPacket.Parser.ParseFrom(bytes);
                    adminMessage = AdminMessage.Parser.ParseFrom(meshPacket.Decoded.Payload);
                }
            }
            catch (InvalidProtocolBufferException) 
            {
                //fromRadio = null;
                //adminMessage = new AdminMessage() { };
                //TODO: Log
            }

            if (adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.None)
                adminMessage = null;
        }

        public (FromRadio? fromRadio, AdminMessage? adminMessage) ParsedMessage => (fromRadio, adminMessage);
    }
}
