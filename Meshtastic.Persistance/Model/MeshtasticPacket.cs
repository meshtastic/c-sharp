using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public abstract class MeshtasticPacket
{
    [Required]
    public uint NodeId { get; set; }

    public virtual Node Node { get; set; }

    [Required]
    public string Payload { get; set; } = String.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
