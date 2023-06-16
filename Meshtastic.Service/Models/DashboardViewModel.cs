using Meshtastic.Persistance.Model;

namespace Meshtastic.Service.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Node> Nodes { get; set; }

        public IEnumerable<TrafficCount> TrafficByPort { get; set; }
        public IEnumerable<TrafficCount> TrafficByNode { get; set; }

    }

    public record TrafficCount(uint Id, string Name, int Count);
}
