using Meshtastic.Persistance.Model;
using Meshtastic.Protobufs;

namespace Meshtastic.Service.Models
{
    public record FromRadioViewModel(Node Node, PortNum Port, string Payload, DateTime Timestamp);
}
