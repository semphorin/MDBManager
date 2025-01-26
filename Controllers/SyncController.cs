using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json.Serialization;
using System.Text.Json;
using YamlDotNet.Core.Tokens;
using Microsoft.VisualBasic;

namespace MDBManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        //should be an absolute path
        private readonly string musicFolder = GetConfiguredMusicPath();


        static public string GetConfiguredMusicPath(){
            string path = Path.Combine(Directory.GetCurrentDirectory(), "musicpath.yaml");
            if (!System.IO.File.Exists(path))
            {
                return "";
            }

            string yamlContent = System.IO.File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);

            return yamlObject["musicPath"];
        }


        // We generate the metadata in Python outside of the webserver for convenience.
        // MDBClient will generate its own metadata.
        // Possibly find a way to run the script on its own whenever the Music folder
        // is updated?
        // Can the webserver update the db on its own at a set interval? (probably)

        // Metadata should be used on the server to determine if a sync is needed.

        // Purely an example method. Retrieves the server's metadata file.
        [HttpGet("getMetadata")]
        public IActionResult ReturnMetadata()
        {
            try
            {
                using (StreamReader reader = new StreamReader("metadata.json"))
                {
                    string temp = reader.ReadToEnd();
                    Dictionary<string,string> metadata = JsonSerializer.Deserialize<Dictionary<string,string>>(temp);
                    return Ok(metadata);
                }
            }
            catch
            {
                return NoContent();
            }
        }


        [HttpPost("sendMetadata")]
        public IActionResult ReceiveMetadata([FromBody] Dictionary<string,string> clientMetadata)
        {
            //open the server's metadata file
            Dictionary<string,string> serverMetadata = new Dictionary<string,string>();
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
                return NoContent();
            }

            Dictionary<string,string> diff = new Dictionary<string,string>();
            foreach (KeyValuePair<string,string> entry in serverMetadata)
            {
                if (!clientMetadata.ContainsKey(entry.Key) || clientMetadata[entry.Key] != entry.Value)
                {
                    diff[entry.Key] = entry.Value;
                }
            }

            return Ok(diff);
        }

        // [HttpGet("download/{*relPath}")]
        // public IActionResult DownloadFile(string relPath)
        // {
        //     var filePath = Path.Combine(musicFolder, relPath);

        //     if (!System.IO.File.Exists(filePath))
        //     {
        //         return NotFound("File not found.");
        //     }

        //     var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        //     var fileName = Path.GetFileName(filePath);
        //     return File(fileStream, "application/octet-stream", fileName);
        // }


        // [HttpGet("gendata")]
        // public IActionResult GenerateMetadata()
        // {

        //     // check for valid music path
        //     if (musicFolder == "" || musicFolder == null){
        //         return NotFound("Please provide a Music Folder.");
        //     }
        //     else if (!Directory.Exists(musicFolder))
        //     {
        //         return NotFound("Music Folder not found.");
        //     }
            
        //     // Begin to create metadata for the server's music.
        //     // Extremely rudimentary, but all it does
        //     // is create json data to compare against.
        //     Dictionary<string, string> metadata = new Dictionary<string, string>();
        //     foreach (string file in Directory.EnumerateFiles(musicFolder, "*", SearchOption.AllDirectories))
        //     {
        //         // EnumerateFiles's searchPattern parameter does NOT support regex.
        //         // The block below will check against a small array of known extension
        //         // types and only continue with enumeration if the file is valid music.
        //         string[] extensions = [".mp3", ".flac", ".ogg"];
        //         bool isMusicFile = false;
        //         foreach (string extension in extensions){
        //             if (file.EndsWith(extension)){isMusicFile = true;}
        //         }
        //         if (!isMusicFile){continue;}
        //         else{isMusicFile = false;}
                
        //         string relPath = Path.GetRelativePath(musicFolder, file);
        //         string hash = ComputeHash(file);
        //         //string hash = "test";
        //         metadata[relPath] = hash;
        //         //Console.WriteLine(metadata[relPath]);
        //     }

        //     return Ok(metadata);
        // }


        // private static string ComputeHash(string filePath)
        // {
        //     using var sha256 = SHA256.Create();
        //     using var stream = System.IO.File.OpenRead(filePath);
        //     var hash = sha256.ComputeHash(stream);
        //     return BitConverter.ToString(hash).Replace("-", "").ToLower();
        // }
    }
}
