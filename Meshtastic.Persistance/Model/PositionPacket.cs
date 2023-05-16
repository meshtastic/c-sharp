using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public class PositionPacket : MeshtasticPacket
{
    [Key]
    public uint PositionId { get; set; }
}
