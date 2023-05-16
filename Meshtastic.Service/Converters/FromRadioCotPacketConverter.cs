using dpp.cot;
using Meshtastic.Data;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;
using System.Xml;
using TheBentern.Tak.Client;

namespace Meshtastic.Service.Converters;

public class FromRadioCotPacketConverter
{
    private readonly FromRadio fromRadio;
    private readonly DeviceStateContainer container;
    private const string AllChatRoomsUid = "DE76CD55-F4A6-4E14-8BAA-C2BB95A90030"; //"8EC57F43-7765-433B-B050-2CBC6372CC89";
    private const string AllChatRoomsName = "All Chat Rooms";
    public FromRadioCotPacketConverter(FromRadio fromRadio, DeviceStateContainer container)
    {
        this.fromRadio = fromRadio;
        this.container = container;
    }

    public CotPacket? Convert()
    {
        CotPacket? packet = null;

        if (fromRadio.GetPayload<Position>() != null)
            packet = GetPositionPacket();
        else if (fromRadio.GetPayload<string>() != null)
            packet = GetChatPacket();

        return packet;
    }

    public CotPacket GetPositionPacket()
    {
        var time = DateTime.UtcNow;
        var position = fromRadio.GetPayload<Position>()!;

        var message = new Message()
        {
            Event = new Event()
            {
                Uid = AllChatRoomsUid,
                Type = "a-f-G-E-V-C",
                How = "m-g",
                Time = time,
                Start = time,
                Stale = time.AddMinutes(2),
                Point = new Point()
                {
                    Lat = Math.Round(position.LatitudeI * 1e-7, 7),
                    Lon = Math.Round(position.LongitudeI * 1e-7, 7),
                    Hae = position.AltitudeHae 
                },
                Detail = new Detail()
                {
                    Contact = new Contact() { Callsign = GetCallsign() },
                    PrecisionLocation = null,
                    Takv = GetTakv(),
                    Group = null,
                    Status = null,
                    Track = null,
                },
                Version = "2.0"
            }
        };
        var xml = new XmlDocument();
        xml.LoadXml(message.ToXmlString());

        return new CotPacket(message.Event, xml);
    }
    public CotPacket GetChatPacket()
    {
        var messageId = Guid.NewGuid().ToString();
        var time = DateTime.UtcNow;

        var message = new Message()
        {
            Event = new Event()
            {
                Uid = $"GeoChat.{AllChatRoomsUid}.AllChatRooms.{messageId}",
                Type = "b-t-f",
                How = "h-g-i-g-o",
                Time = time,
                Start = time,
                Stale = time.AddMinutes(15),
                Point = new Point() { Lat = 0, Lon = 0 },
                Detail = new Detail()
                {
                    Contact = new Contact() { Callsign = GetCallsign() },
                    PrecisionLocation = null,
                    Takv = GetTakv(),
                    Group = null,
                    Status = null,
                    Track = null,
                },
                Version = "2.0"
            }
        };

        var xml = new XmlDocument();

        var chatElement = GetChatElement(messageId, xml);
        var remarksElement = GetRemarksElement(xml, fromRadio.GetPayload<string>() ?? String.Empty);
        var linkElement = GetLinkElement(xml);

        xml.LoadXml(message.ToXmlString());
        xml.GetElementsByTagName("detail")[0]!.AppendChild(chatElement);
        xml.GetElementsByTagName("detail")[0]!.AppendChild(remarksElement);
        xml.GetElementsByTagName("detail")[0]!.AppendChild(linkElement);

        return new CotPacket(message.Event, xml);
    }

    private string GetCallsign()
    {
        return container.GetNodeDisplayName(fromRadio.Packet.From, shortName: false, hideNodeNum: true) ?? $"Meshtastic {fromRadio.Packet.From}";
    }

    private Takv GetTakv()
    {
        return new Takv() { Device = container.Nodes.Find(n => n.Num == fromRadio.Packet.From)?.User?.HwModel.ToString() ?? "Meshtastic-Device", Platform = "Meshtastic" };
    }

    private static XmlElement GetRemarksElement(XmlDocument xml, string text)
    {
        var remarksElement = xml.CreateElement("remarks");
        remarksElement.SetAttribute("source", $"BAO.F.ATAK.{AllChatRoomsUid}");
        remarksElement.SetAttribute("to", AllChatRoomsName);
        //TODO: time
        remarksElement.InnerText = text;

        return remarksElement;
    }

    private static XmlElement GetLinkElement(XmlDocument xml)
    {
        var linkElement = xml.CreateElement("link");
        linkElement.SetAttribute("uiid", AllChatRoomsUid);
        linkElement.SetAttribute("type", "a-f-G-E-V-C");
        linkElement.SetAttribute("relation", "p-p");

        return linkElement;
    }

    private XmlElement GetChatElement(string messageId, XmlDocument xml)
    {
        var idAttribute = xml.CreateAttribute("id");
        idAttribute.Value = AllChatRoomsName;

        var uid0Attribute = xml.CreateAttribute("uid0");
        uid0Attribute.Value = AllChatRoomsUid;

        var uid1Attribute = xml.CreateAttribute("uid1");
        uid1Attribute.Value = AllChatRoomsName;

        var chatGroupElement = xml.CreateElement("chatgrp");
        chatGroupElement.Attributes.SetNamedItem(idAttribute);
        chatGroupElement.Attributes.SetNamedItem(uid0Attribute);
        chatGroupElement.Attributes.SetNamedItem(uid1Attribute);

        var messageIdAttribute = xml.CreateAttribute("messageId");
        messageIdAttribute.Value = messageId;

        var chatroomAttribute = xml.CreateAttribute("chatroom");
        chatroomAttribute.Value = AllChatRoomsName;

        var sendCallsignAttribute = xml.CreateAttribute("senderCallsign");
        sendCallsignAttribute.Value = container.GetNodeDisplayName(fromRadio.Packet.From, shortName: false, hideNodeNum: true)
            ?? $"Meshtastic {fromRadio.Packet.From}";

        var chatElement = xml.CreateElement("__chat");
        chatElement.Attributes.SetNamedItem(idAttribute);
        chatElement.Attributes.SetNamedItem(chatroomAttribute);
        chatElement.Attributes.SetNamedItem(sendCallsignAttribute);
        chatElement.Attributes.SetNamedItem(messageIdAttribute);
        chatElement.AppendChild(chatGroupElement);

        return chatElement;
    }
}
