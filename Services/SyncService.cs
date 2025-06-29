using YamlDotNet.Serialization;
using System.Text.Json;
using System.IO.Compression;
using MDBManager.Services;


public interface ISyncService{
    Task<Dictionary<string,string>> ReceiveMetadataAsync(Dictionary<string,string> clientMetadata);
    Task<Dictionary<string,string>> ReturnMetadataAsync();
    byte[] DownloadDiff(Dictionary<string,string> fileDiff);
    string GetConfiguredMusicPath();
}
public class SyncService : ISyncService
{
    private readonly IMetadataService _metadataService;

    public SyncService(IMetadataService metadataService)
    {
        _metadataService = metadataService;
    }

    public string GetConfiguredMusicPath()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Config/musicpath.yaml");
        if (!System.IO.File.Exists(path))
            return "";

        string yamlContent = System.IO.File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);

        return yamlObject["musicPath"];
    }

    public async Task<Dictionary<string,string>> ReceiveMetadataAsync(Dictionary<string,string> clientMetadata)
    {
        try
        {
            return await _metadataService.GetMetadataDiffAsync(clientMetadata);
        }
        catch
        {
            return new Dictionary<string,string>();
        }
    }

    public async Task<Dictionary<string,string>> ReturnMetadataAsync()
    {
        try
        {
            return await _metadataService.GetAllMetadataAsync();
        }
        catch
        {
            return new Dictionary<string,string>();
        }
    }

    public byte[] DownloadDiff(Dictionary<string,string> fileDiff)
    {
        if (fileDiff.Count == 0)
        {
            return [];
        }

        // create zip file in memory only - no temporary files
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (KeyValuePair<string,string> entry in fileDiff)
                {
                    string filePath = Path.Combine(this.GetConfiguredMusicPath(), entry.Key);
                    // probably shouldn't reach this point, but just in case
                    if (System.IO.File.Exists(filePath))
                    {
                        ZipArchiveEntry fileEntry = archive.CreateEntry(entry.Key);
                        using (Stream entryStream = fileEntry.Open())
                        {
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }
            // return the byte array directly from memory - no file I/O needed
            return memoryStream.ToArray();
        }
    }
    // private static string ComputeHash(string filePath)
    // {
    //     using var sha256 = SHA256.Create();
    //     using var stream = System.IO.File.OpenRead(filePath);
    //     var hash = sha256.ComputeHash(stream);
    //     return BitConverter.ToString(hash).Replace("-", "").ToLower();
    // }
}