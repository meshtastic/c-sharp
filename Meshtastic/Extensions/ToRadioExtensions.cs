using Meshtastic.Protobufs;

namespace Meshtastic.Extensions;

public static class ToRadioExtensions
{
    private static bool IsValidMeshPacket(ToRadio toRadio)
    {
        return toRadio.PayloadVariantCase == ToRadio.PayloadVariantOneofCase.Packet &&
            toRadio.Packet?.PayloadVariantCase == MeshPacket.PayloadVariantOneofCase.Decoded &&
            toRadio.Packet?.Decoded?.Payload != null;
    }

    public static TResult? GetPayload<TResult>(this ToRadio toRadio) where TResult : class
    {
        if (typeof(TResult) == typeof(XModem) && toRadio.PayloadVariantCase == ToRadio.PayloadVariantOneofCase.XmodemPacket)
            return toRadio.XmodemPacket as TResult;

        if (!IsValidMeshPacket(toRadio))
            return null;

        if (typeof(TResult) == typeof(AdminMessage) && toRadio.Packet?.Decoded?.Portnum == PortNum.AdminApp)
            return AdminMessage.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(RouteDiscovery) && toRadio.Packet?.Decoded?.Portnum == PortNum.TracerouteApp)
            return RouteDiscovery.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Routing) && toRadio.Packet?.Decoded?.Portnum == PortNum.RoutingApp)
            return Routing.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Position) && toRadio.Packet?.Decoded?.Portnum == PortNum.PositionApp)
            return Position.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Telemetry) && toRadio.Packet?.Decoded?.Portnum == PortNum.TelemetryApp)
            return Telemetry.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(NodeInfo) && toRadio.Packet?.Decoded?.Portnum == PortNum.NodeinfoApp)
            return NodeInfo.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(Waypoint) && toRadio.Packet?.Decoded?.Portnum == PortNum.WaypointApp)
            return NodeInfo.Parser.ParseFrom(toRadio.Packet?.Decoded?.Payload) as TResult;

        else if (typeof(TResult) == typeof(string) && toRadio.Packet?.Decoded?.Portnum == PortNum.TextMessageApp)
            return toRadio.Packet?.Decoded?.Payload.ToStringUtf8() as TResult;

        else if (typeof(TResult) == typeof(string) && toRadio.Packet?.Decoded?.Portnum == PortNum.SerialApp)
            return toRadio.Packet?.Decoded?.Payload.ToStringUtf8() as TResult;

        return null;
    }
}
