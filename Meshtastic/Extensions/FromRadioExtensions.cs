using Google.Protobuf;
using Meshtastic.Protobufs;

namespace Meshtastic.Extensions;

public static class FromRadioExtensions
{
    private static bool IsValidMeshPacket(FromRadio fromRadio)
    {
        return fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.Packet &&
            fromRadio.Packet?.PayloadVariantCase == MeshPacket.PayloadVariantOneofCase.Decoded &&
            fromRadio.Packet?.Decoded?.Payload != null;
    }

    public static TResult? GetMessage<TResult>(this FromRadio fromRadio) where TResult : class
    {
        if (!IsValidMeshPacket(fromRadio))
            return null;

        if (typeof(TResult) == typeof(AdminMessage) && fromRadio.Packet?.Decoded?.Portnum == PortNum.AdminApp)
            return AdminMessage.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(RouteDiscovery) && fromRadio.Packet?.Decoded?.Portnum == PortNum.TracerouteApp)
            return RouteDiscovery.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Routing) && fromRadio.Packet?.Decoded?.Portnum == PortNum.RoutingApp)
            return Routing.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        return null;
    }
}
