using Google.Protobuf;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Data
{
    public class FromDeviceMessage
    {
        public FromDeviceMessage(ILogger logger)
        {
            Logger = logger;
        }

        public FromRadio? ParsedFromRadio(byte[] bytes) 
        {
            try
            {
                var fromRadio = FromRadio.Parser.ParseFrom(bytes);
                if (fromRadio?.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.None)
                {
                    fromRadio = null;
                }

                return fromRadio;
            }
            catch (InvalidProtocolBufferException ex)
            {
                Logger.LogTrace(ex.ToString());
                return null;
            }
        }

        public ILogger Logger { get; }
    }
}
