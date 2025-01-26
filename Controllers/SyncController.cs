using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;


namespace MDBManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        // We generate the metadata in Python outside of the webserver for convenience.
        // MDBClient will generate its own metadata.
        // Possibly find a way to run the script on its own whenever the Music folder
        // is updated?
        // Can the webserver update the db on its own at a set interval? (probably)

        // Metadata should be used on the server to determine if a sync is needed.
        private readonly ISyncService _syncService;
        private readonly IMemoryCache _memoryCache;
        public SyncController(ISyncService syncService, IMemoryCache memoryCache)
        {
            _syncService = syncService;
            _memoryCache = memoryCache;
        }

        [HttpGet("getMetadata")]
        public IActionResult ReturnMetadata()
        {
            try
            {
                return Ok(_syncService.ReturnMetadata());
            }
            catch
            {
                return NoContent();
            }
        }


        [HttpPost("sendMetadata")]
        [Authorize]
        public IActionResult ReceiveMetadata([FromBody] Dictionary<string,string> clientMetadata)
        {
            //if the output is {}, client knows sync isn't needed
            Dictionary<string,string> diff = _syncService.ReceiveMetadata(clientMetadata);
            _memoryCache.Set("diff", diff);
            return Ok(diff);
        }


        [HttpGet("downloadDiff")]
        [Authorize]
        public IActionResult DownloadDiff()
        {
            _memoryCache.TryGetValue("diff", out Dictionary<string,string>? diff);
            byte[] fileBytes = _syncService.DownloadDiff(diff);
            return File(fileBytes, "application/zip", "diff.zip");
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



    }
}
