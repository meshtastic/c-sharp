using System.ComponentModel.DataAnnotations;

namespace Meshtastic.Persistance.Model;

public class Node
{
    [Key]
    public uint Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string HardwareModel { get; set; }

    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public int? LastAltitude { get; set; }
    public int? LastBatteryLevel { get; set; }
    public float? LastAirUtilTx { get; set; }
    public float? LastChannelUtilTx { get; set; }

    public string? TAKCallsign { get; set; }
    public string TAKTeam { get; set; } = "Cyan";
    public string TAKRole { get; set; } = "Team Member";
    public string TAKUnitType { get; set; } = "a-f-G-E-V-C"; // Ground
    public float SNR { get; set; }
    public float RSSI { get; set; }
    public DateTime LastHeardFrom { get; set; }
}
