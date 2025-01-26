using YamlDotNet.Serialization;
using System.Text.Json;
using System.IO.Compression;


public interface ISyncService{
    Dictionary<string,string> ReceiveMetadata(Dictionary<string,string> clientMetadata);
    Dictionary<string,string> ReturnMetadata();
    byte[] DownloadDiff(Dictionary<string,string> fileDiff);
    string GetConfiguredMusicPath();
}
public class SyncService : ISyncService
{
    public SyncService()
    {

    }

    public string GetConfiguredMusicPath()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Config/musicpath.yaml");
        if (!System.IO.File.Exists(path))
        {
            return "";
        }

        string yamlContent = System.IO.File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);

        return yamlObject["musicPath"];
    }

    public Dictionary<string,string> ReceiveMetadata(Dictionary<string,string> clientMetadata)
    {
        Dictionary<string,string>? diff = new Dictionary<string,string>();
        Dictionary<string,string>? serverMetadata = new Dictionary<string,string>();
        try
        {
            using (StreamReader reader = new StreamReader("metadata.json"))
            {
                string temp = reader.ReadToEnd();
                serverMetadata = JsonSerializer.Deserialize<Dictionary<string,string>>(temp);
            }
        }
        catch
        {
            return new Dictionary<string,string>();
        }

        if (serverMetadata is not null)
        {
            foreach (KeyValuePair<string,string> entry in serverMetadata)
            {
                if (!clientMetadata.ContainsKey(entry.Key) || clientMetadata[entry.Key] != entry.Value)
                {
                    diff[entry.Key] = entry.Value;
                }
            }
        }

        //if the output is {}, client knows sync isn't needed
        return diff;
    }

    public Dictionary<string,string> ReturnMetadata()
    {
        using (StreamReader reader = new StreamReader("metadata.json"))
        {
            string temp = reader.ReadToEnd();
            // force compiler to ignore null dict (if check in place to prevent a null return)
            Dictionary<string,string>? metadata = JsonSerializer.Deserialize<Dictionary<string,string>>(temp);
            if (metadata != null){return metadata;}
            else{return new Dictionary<string,string>();}
        }
    }

    public byte[] DownloadDiff(Dictionary<string,string> fileDiff)
    {
        if (fileDiff.Count == 0)
        {
            return [];
        }

        // create zip file on system
        using (FileStream fs = new FileStream("diff.zip", FileMode.Create))
        {
            // create zip file in memory
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (KeyValuePair<string,string> entry in fileDiff)
                {
                    string filePath = Path.Combine(this.GetConfiguredMusicPath(), entry.Key);
                    // probably shouldn't reach this point, but just in case
                    if (!System.IO.File.Exists(filePath))
                    {
                        continue;
                    }

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
        // zip file is packaged for client use. folder structure is preserved.
        return System.IO.File.ReadAllBytes("diff.zip");
    }
    // private static string ComputeHash(string filePath)
    // {
    //     using var sha256 = SHA256.Create();
    //     using var stream = System.IO.File.OpenRead(filePath);
    //     var hash = sha256.ComputeHash(stream);
    //     return BitConverter.ToString(hash).Replace("-", "").ToLower();
    // }
}