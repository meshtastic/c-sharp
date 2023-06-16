using dpp.cot;
using Meshtastic.Data;
using Meshtastic.Data.MessageFactories;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using TheBentern.Tak.Client;

namespace Meshtastic.Service.Converters;

public class CotPacketToRadioConverter
{
    private readonly Event cotEvent;
    private readonly XmlDocument raw;
    private readonly DeviceStateContainer container;
    private readonly ToRadioMessageFactory toRadioFactory;
    private readonly double divisor = 1e-7d;
    private readonly MD5 md5Hasher = MD5.Create();

    public CotPacketToRadioConverter(CotPacket cotPacket, DeviceStateContainer container)
    {
        this.cotEvent = cotPacket.Event;
        this.raw = cotPacket.Raw;
        this.container = container;
        this.toRadioFactory = new ToRadioMessageFactory();
    }

    private uint HashCallSignFromNum()
    {
        var callsign = cotEvent.Detail.Contact?.Callsign ?? 
            raw.GetElementsByTagName("__chat")[0]?.Attributes?["senderCallsign"]?.Value;

        if (callsign == null)
            return 0;

        var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(callsign));
        return BitConverter.ToUInt32(hashed, 0);
    }

    private string? GetRemarksText()
    {
        if (raw.GetElementsByTagName("remarks").Count > 0)
            return raw.GetElementsByTagName("remarks")[0]?.InnerText;

        return null;
    }

    public bool WantConversion()
    {
        if (cotEvent == null)
            return false;

        return true;
    }

    public ToRadio ConvertToRadio()
    {
        // TODO: to / from DM management
        if (cotEvent.Uid.StartsWith("GeoChat") && !String.IsNullOrWhiteSpace(GetRemarksText()))
        {
            var textMessageFactory = new TextMessageFactory(container);
            
            var message = textMessageFactory.CreateTextMessagePacket(GetRemarksText()!);
            message.From = HashCallSignFromNum();
            return toRadioFactory.CreateMeshPacketMessage(message);
        } 
        else
        {
            var positionMessageFactory = new PositionMessageFactory(container);
            var message = positionMessageFactory.CreatePositionPacket(new Position()
            {
                LatitudeI = cotEvent.Point.Lat != 0 ? Convert.ToInt32(cotEvent.Point.Lat / divisor) : 0,
                LongitudeI = cotEvent.Point.Lon != 0 ? Convert.ToInt32(cotEvent.Point.Lon / divisor) : 0,
                Altitude = Convert.ToInt32(Math.Round(cotEvent.Point.Hae)),
                Time = DateTime.Now.GetUnixTimestamp(),
                Timestamp = cotEvent.Time.GetUnixTimestamp(),
            });
            message.From = HashCallSignFromNum();
            return toRadioFactory.CreateMeshPacketMessage(message);
        }
    }
}
