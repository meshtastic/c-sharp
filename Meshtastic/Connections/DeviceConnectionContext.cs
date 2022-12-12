namespace Meshtastic.Connections;

public class DeviceConnectionContext 
{
    private readonly string? port;
    private readonly string? host;

    public DeviceConnectionContext(string? port, string? host)
    {
        this.port = port;
        this.host = host;
    }

    public string DisplayName => !String.IsNullOrWhiteSpace(this.host) ?
        $"via tcp to host {this.host}" :
        $"to port {this.port}";

    public DeviceConnection GetDeviceConnection() 
    {
        if (!String.IsNullOrWhiteSpace(this.host)) 
            return new TcpConnection(this.host);
        else if (!String.IsNullOrWhiteSpace(this.port)) 
            return new SerialConnection(this.port);
      
        throw new ApplicationException("Please specify a port or host to connect to a device");
    }
}