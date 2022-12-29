using Meshtastic.Connections;
using Microsoft.Extensions.Logging;

namespace Meshtastic.Cli;

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
        $"via tcp to hostname/IP {this.host}" : $"to port {this.port}";

    public DeviceConnection GetDeviceConnection(ILogger logger)
    {
        if (!String.IsNullOrWhiteSpace(this.host))
            return new TcpConnection(logger, this.host);
        else if (!String.IsNullOrWhiteSpace(this.port))
            return new SerialConnection(logger, this.port);

        var ports = SerialConnection.ListPorts();

        if (!ports.Any())
            throw new InvalidOperationException("No port or hostname specified and could not find available serial ports");
        if (ports.Length == 1)
            return new SerialConnection(logger, ports.First());

        var selectedPort = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("No port or host specified, please select a serial port")
            .AddChoices(ports));

        return new SerialConnection(logger, selectedPort);
    }
}