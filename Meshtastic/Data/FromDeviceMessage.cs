using Google.Protobuf;
using Meshtastic.Protobufs;

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
                fromRadio = FromRadio.Parser.ParseFrom(bytes);
                if (fromRadio?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.None)
                {
                    fromRadio = null;
                    var meshPacket = MeshPacket.Parser.ParseFrom(bytes);

                    if (meshPacket.Decoded?.Payload != null)
                        adminMessage = AdminMessage.Parser.ParseFrom(meshPacket.Decoded.Payload);
                }

                if (fromRadio?.Packet?.Decoded?.Portnum == PortNum.AdminApp && 
                    fromRadio?.Packet?.Decoded?.Payload != null)
                    adminMessage = AdminMessage.Parser?.ParseFrom(fromRadio.Packet.Decoded.Payload);
            }
            catch (InvalidProtocolBufferException)
            {
                //TODO: Log
            }

            if (adminMessage?.PayloadVariantCase == AdminMessage.PayloadVariantOneofCase.None)
                adminMessage = null;
        }

        public (FromRadio? fromRadio, AdminMessage? adminMessage) ParsedMessage => (fromRadio, adminMessage);
    }
}
