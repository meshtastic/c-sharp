using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public class WaypointPacket : MeshtasticPacket
{
    [Key]
    public uint WaypointId { get; set; }
}
