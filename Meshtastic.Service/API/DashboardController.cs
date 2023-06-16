using Meshtastic.Persistance;
using Meshtastic.Persistance.Model;
using Meshtastic.Protobufs;
using Meshtastic.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Meshtastic.Service.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private MeshtasticDbContext Context { get; }

        public DashboardController(MeshtasticDbContext context)
        {
            Context = context;
        }

        [HttpGet]
        public async Task<DashboardViewModel> Get()
        {
            var nodes = await Context.Nodes
                .OrderByDescending(n => n.LastHeardFrom)
                .ToListAsync();

            return new DashboardViewModel
            {
                Nodes = nodes,
                TrafficByPort = new[] 
                { 
                    new TrafficCount((uint)PortNum.TextMessageApp, "Text", await Context.Texts.CountAsync()),
                    new TrafficCount((uint)PortNum.TelemetryApp, "Telemetry", await Context.Telemetries.CountAsync()),
                    new TrafficCount((uint)PortNum.PositionApp, "Position", await Context.Positions.CountAsync()),
                    new TrafficCount((uint)PortNum.WaypointApp, "Waypoint", await Context.Waypoints.CountAsync()),
                },
                TrafficByNode = nodes.Select(n =>
                {
                    var count = Context.Telemetries.Where(p => p.NodeId == n.Id).Count() +
                        Context.Telemetries.Where(p => p.NodeId == n.Id).Count() +
                        Context.Positions.Where(p => p.NodeId == n.Id).Count() +
                        Context.Waypoints.Where(p => p.NodeId == n.Id).Count();

                    return new TrafficCount(n.Id, n.Name, count);
                })
            };
        }
    }
}
