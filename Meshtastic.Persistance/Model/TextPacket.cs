using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public class TextPacket : MeshtasticPacket
{
    [Key]
    public uint TextId { get; set; }
}
