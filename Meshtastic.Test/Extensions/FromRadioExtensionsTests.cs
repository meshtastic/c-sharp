using Google.Protobuf;
using Meshtastic.Extensions;
using Meshtastic.Protobufs;

namespace Meshtastic.Test.Extensions
{
    [TestFixture]
    public class FromRadioExtensionsTests
    {
        [Test]
        public void GetMessage_Should_ReturnNullForBadPackets()
        {
            FromRadio fromRadio = new();
            var result = fromRadio.GetPayload<AdminMessage>();
            result.Should().BeNull();
            fromRadio.Packet = new MeshPacket();
            result = fromRadio.GetPayload<AdminMessage>();
            result.Should().BeNull();
            fromRadio.Packet = new MeshPacket
            {
                Decoded = new Protobufs.Data()
            };
            result = fromRadio.GetPayload<AdminMessage>();
            result.Should().BeNull();
        }

        [Test]
        public void GetMessage_Should_ReturnValidAdminMessage()
        {
            FromRadio fromRadio = new();
            var result = fromRadio.GetPayload<AdminMessage>();
            fromRadio.Packet = new MeshPacket
            {
                Decoded = new Protobufs.Data()
                {
                    Portnum = PortNum.AdminApp,
                    Payload = new AdminMessage().ToByteString()
                }
            };
            result = fromRadio.GetPayload<AdminMessage>();
            result.Should().NotBeNull();
            result.Should().BeOfType<AdminMessage>();
        }

        [Test]
        public void GetMessage_Should_ReturnValidRouteDiscovery()
        {
            FromRadio fromRadio = new();
            var result = fromRadio.GetPayload<RouteDiscovery>();
            fromRadio.Packet = new MeshPacket
            {
                Decoded = new Protobufs.Data()
                {
                    Portnum = PortNum.TracerouteApp,
                    Payload = new RouteDiscovery().ToByteString()
                }
            };
            result = fromRadio.GetPayload<RouteDiscovery>();
            result.Should().NotBeNull();
            result.Should().BeOfType<RouteDiscovery>();
        }

        [Test]
        public void GetMessage_Should_ReturnValidRouting()
        {
            FromRadio fromRadio = new();
            var result = fromRadio.GetPayload<Routing>();
            fromRadio.Packet = new MeshPacket
            {
                Decoded = new Protobufs.Data()
                {
                    Portnum = PortNum.RoutingApp,
                    Payload = new Routing().ToByteString()
                }
            };
            result = fromRadio.GetPayload<Routing>();
            result.Should().NotBeNull();
            result.Should().BeOfType<Routing>();
        }


        [Test]
        public void GetMessage_Should_ReturnValidXModemPacket()
        {
            FromRadio fromRadio = new()
            {
                XmodemPacket = new XModem()
                {
                    Control = XModem.Types.Control.Stx
                }
            };
            var result = fromRadio.GetPayload<XModem>();
            result.Should().NotBeNull();
            result.Should().BeOfType<XModem>();
            result!.Control.Should().Be(XModem.Types.Control.Stx);
        }
    }
}
