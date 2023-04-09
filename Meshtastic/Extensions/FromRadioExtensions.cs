using Meshtastic.Data.MessageFactories;
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

    public static TResult? GetPayload<TResult>(this FromRadio fromRadio) where TResult : class
    {
        if (typeof(TResult) == typeof(XModem) && fromRadio.PayloadVariantCase == FromRadio.PayloadVariantOneofCase.XmodemPacket)
            return fromRadio.XmodemPacket as TResult;

        if (!IsValidMeshPacket(fromRadio))
            return null;

        if (typeof(TResult) == typeof(AdminMessage) && fromRadio.Packet?.Decoded?.Portnum == PortNum.AdminApp)
            return AdminMessage.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(RouteDiscovery) && fromRadio.Packet?.Decoded?.Portnum == PortNum.TracerouteApp)
            return RouteDiscovery.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Routing) && fromRadio.Packet?.Decoded?.Portnum == PortNum.RoutingApp)
            return Routing.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Position) && fromRadio.Packet?.Decoded?.Portnum == PortNum.PositionApp)
            return Position.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Telemetry) && fromRadio.Packet?.Decoded?.Portnum == PortNum.TelemetryApp)
            return Telemetry.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(NodeInfo) && fromRadio.Packet?.Decoded?.Portnum == PortNum.NodeinfoApp)
            return NodeInfo.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Waypoint) && fromRadio.Packet?.Decoded?.Portnum == PortNum.WaypointApp)
            return NodeInfo.Parser.ParseFrom(fromRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(string) && fromRadio.Packet?.Decoded?.Portnum == PortNum.TextMessageApp)
            return fromRadio.Packet?.Decoded?.Payload.ToStringUtf8() as TResult;

        else if (typeof(TResult) == typeof(string) && fromRadio.Packet?.Decoded?.Portnum == PortNum.SerialApp)
            return fromRadio.Packet?.Decoded?.Payload.ToStringUtf8() as TResult;

        return null;
    }
}
