using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public class TelemetryPacket : MeshtasticPacket
{
    [Key]
    public uint TelemetryId { get; set; }
}
