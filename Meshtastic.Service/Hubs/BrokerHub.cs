using Meshtastic.Service.Models;
using Microsoft.AspNetCore.SignalR;

namespace Meshtastic.Service.Hubs;

public class BrokerHub : Hub<IBrokerHubClient>
{
}

public interface IBrokerHubClient
{
    Task FromRadioReceived(FromRadioViewModel fromRadio);
    Task ConnectionStateChanged(ConnectionStatusViewModel connectionStatus);
}