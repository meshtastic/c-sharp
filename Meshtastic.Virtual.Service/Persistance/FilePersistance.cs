using static System.Environment;

namespace Meshtastic.Virtual.Service.Persistance;

public class FilePersistance(ILogger<IFilePersistance> logger) : IFilePersistance
{
    private static readonly string basePath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "meshtastic");

    public async Task<bool> Save(string fileName, byte[] data)
    {
        try
        {
            // Ensure the directory and all its parents exist.
            Directory.CreateDirectory(basePath);
            await File.WriteAllBytesAsync(fileName, data);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving file {FileName}", fileName);
            return false;
        }
    }

    public async Task<byte[]> Load(string fileName)
    {
        return await File.ReadAllBytesAsync(fileName);
    }

    public async Task<bool> Exists(string fileName)
    {
        return await Task.FromResult(File.Exists(fileName));
    }

    public async Task<bool> Delete(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                return true;
            }
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return false;
        }
    }
}