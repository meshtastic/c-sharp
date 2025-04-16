namespace Meshtastic.Virtual.Service.Persistance;

public interface IFilePersistance
{
    Task<bool> Delete(string fileName);
    Task<bool> Exists(string fileName);
    Task<byte[]> Load(string fileName);
    Task<bool> Save(string fileName, byte[] data);
}
