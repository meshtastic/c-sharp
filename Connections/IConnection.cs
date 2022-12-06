namespace Meshtastic.Connections;

public interface IConnection
{
    // Task Disconnect();
    Task WriteToRadio(byte[] data);
}