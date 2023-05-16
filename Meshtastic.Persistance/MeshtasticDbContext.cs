using Meshtastic.Persistance.Model;
using Microsoft.EntityFrameworkCore;

namespace Meshtastic.Persistance;

public class MeshtasticDbContext : DbContext
{

    public MeshtasticDbContext(DbContextOptions<MeshtasticDbContext> options)
    : base(options)
    {
    }

    public MeshtasticDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Filename=Meshtastic.db");
    }

    public DbSet<Node> Nodes { get; set; }
    public DbSet<TelemetryPacket> Telemetries { get; set; }
    public DbSet<NodeInfoPacket> NodeInfos { get; set; }
    public DbSet<PositionPacket> Positions { get; set; }
    public DbSet<TextPacket> Texts { get; set; }
    public DbSet<WaypointPacket> Waypoints { get; set; }
}