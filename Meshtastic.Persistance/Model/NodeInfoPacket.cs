using Meshtastic.Persistance.Model;
using System.ComponentModel.DataAnnotations;

public class NodeInfoPacket : MeshtasticPacket
{
    [Key]
    public uint NodeInfoId { get; set; }
}