namespace Meshtastic.Service.Models;

public class ConnectionStatusViewModel
{
    public ConnectionType ConnectionType { get; set; }
    public string Host { get; set; }
    public bool IsConnected { get; set; }
}

public enum ConnectionType
{
    Meshtastic = 0,
    TAK = 1,
}
