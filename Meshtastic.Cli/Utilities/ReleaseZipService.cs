using Meshtastic.Cli.Mappings;
using Meshtastic.Protobufs;
using System.IO.Compression;

namespace Meshtastic.Cli.Utilities;

public class ReleaseZipService
{
	public ReleaseZipService()
	{
	}

	public string TempDirectory => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

	public async Task<string> ExtractUpdateBinary(MemoryStream memoryStream, HardwareModel hardwareModel)
	{
        var result = new ZipArchive(memoryStream);
		var updateBinEntry = result.Entries.First(entry => MatchUpdateBinary(entry, hardwareModel));
		var localPath = Path.Join(TempDirectory, "Meshtast.Cli");
		Directory.CreateDirectory(localPath);
		var filePath = Path.Join(localPath, updateBinEntry.Name);
		if (!File.Exists(filePath))
		{
            using var fileStream = File.Create(filePath);
            await updateBinEntry.Open().CopyToAsync(fileStream);
        }
		return filePath;
	}

	private bool MatchUpdateBinary(ZipArchiveEntry entry, HardwareModel hardwareModel)
	{
        var isNrf = HardwareModelMappings.NrfHardwareModels.Contains(hardwareModel);
        var targetName = HardwareModelMappings.FileNameMappings[hardwareModel];
        if (isNrf)
			return entry.Name.Contains(targetName) && entry.Name.EndsWith(".uf2");
        else
            return entry.Name.Contains(targetName) && entry.Name.EndsWith("update.bin");
    }
}
