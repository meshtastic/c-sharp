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

/*
<event version="2.0" type="a-f-G-E-V-C" uid="8EC57F43-7765-433B-B050-2CBC6372CC89"
  time="2023-04-14T11:34:28Z" start="2023-04-14T11:34:28Z" stale="2023-04-14T11:36:28Z" how="m-g">
  <point lat="34.805606" lon="-92.481731" ce="9999999" hae="106.9" le="9999999" />
  <detail>
    <contact endpoint="*:-1:stcp" callsign="Echo Mike" />
    <__group name="Cyan" role="RTO" />
    <precisionlocation geopointsrc="GPS" altsrc="???" />
    <status battery="56" />
    <takv device="iPhone" platform="iTAK" os="16.4.1" version="2.5.0.604" />
    <track />
  </detail>
</event>
<event version="2.0"
  uid="GeoChat.8EC57F43-7765-433B-B050-2CBC6372CC89.All Chat Rooms.6AFFCE82-FA4B-407B-93FB-65562F5DD324"
  type="b-t-f" how="h-g-i-g-o" time="2023-04-14T12:48:11.000Z" start="2023-04-14T12:48:11.000Z"
  stale="2023-04-14T12:50:11.000Z">
  <point lat="34.805622" lon="-92.481746" hae="9999999.0" ce="9999999.0" le="9999999.0" />
  <detail>
    <__chat parent="RootContactGroup" groupOwner="false"
      messageId="6AFFCE82-FA4B-407B-93FB-65562F5DD324" chatroom="All Chat Rooms" id="All Chat Rooms"
      senderCallsign="Echo Mike">
      <chatgrp uid0="8EC57F43-7765-433B-B050-2CBC6372CC89" uid1="All Chat Rooms" id="All Chat Rooms" />
    </__chat>
    <link uid="8EC57F43-7765-433B-B050-2CBC6372CC89" type="a-f-G-E-V-C" relation="p-p" />
    <remarks source="BAO.F.ATAK.8EC57F43-7765-433B-B050-2CBC6372CC89" to="All Chat Rooms"
      time="2023-04-14T12:48:11Z">Test results </remarks>
    <__serverdestination destinations="192.168.1.168:4242:tcp:8EC57F43-7765-433B-B050-2CBC6372CC89" />
    <_flow-tags_ TAK-Server-dd4055d128d5416e826423948c66e412="2023-04-14T12:48:11Z" />
  </detail>
</event>
 */
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
