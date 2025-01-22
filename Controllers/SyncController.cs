using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;
using System.IO;
using System.Security.Cryptography;
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


        [HttpGet("metadata")]
        public IActionResult GetMetadata()
        {
            // check for valid music path
            if (musicFolder == "" || musicFolder == null){
                return NotFound("Please provide a Music Folder.");
            }
            else if (!Directory.Exists(musicFolder))
            {
                return NotFound("Music Folder not found.");
            }
            
            // Begin to create metadata for the server's music.
            // Extremely rudimentary, but all it does
            // is create json data to compare against.
            // TODO: figure out a way to find out if we need to
            // regenerate metadata (instead of doing it every time).
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            foreach (string file in Directory.EnumerateFiles(musicFolder, "*", SearchOption.AllDirectories))
            {
                // EnumerateFiles's searchPattern parameter does NOT support regex.
                // The block below will check against a small array of known extension
                // types and only continue with enumeration if the file is valid music.
                string[] extensions = [".mp3", ".flac", ".ogg"];
                bool isMusicFile = false;
                foreach (string extension in extensions){
                    if (file.EndsWith(extension)){isMusicFile = true;}
                }
                if (!isMusicFile){continue;}
                else{isMusicFile = false;}
                
                string relPath = Path.GetRelativePath(musicFolder, file);
                string hash = ComputeHash(file);
                //string hash = "test";
                metadata[relPath] = hash;
            }

            return Ok(metadata);
        }


        [HttpGet("download/{*relPath}")]
        public IActionResult DownloadFile(string relPath)
        {
            var filePath = Path.Combine(musicFolder, relPath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(filePath);
            return File(fileStream, "application/octet-stream", fileName);
        }


        private static string ComputeHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = System.IO.File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
