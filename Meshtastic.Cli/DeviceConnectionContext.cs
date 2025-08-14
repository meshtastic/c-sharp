using Meshtastic.Connections;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Meshtastic.Cli;

[ExcludeFromCodeCoverage(Justification = "Requires hardware")]
public class DeviceConnectionContext(string? port, string? host)
{
    public readonly string? Port = port;
    public readonly string? Host = host;

    public DeviceConnection GetDeviceConnection(ILogger logger)
    {
        if (this.Port == "SIMPORT")
            return new SimulatedConnection(logger);

        if (!String.IsNullOrWhiteSpace(this.Host))
            return new TcpConnection(logger, this.Host);
        else if (!String.IsNullOrWhiteSpace(this.Port))
            return new SerialConnection(logger, this.Port);

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