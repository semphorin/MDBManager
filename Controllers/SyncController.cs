using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;


namespace MDBManager.Controllers
{
    [ApiController]
    [Authorize]
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
        private readonly ILogger<SyncController> _logger;
        
        public SyncController(ISyncService syncService, IMemoryCache memoryCache, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Returns the server's current music metadata as a JSON object.
        /// If an error occurs, returns a 500 status code with an error message.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetMetadata")]
        public async Task<IActionResult> GetMetadata()
        {
            try
            {
                var metadata = await _syncService.ReturnMetadataAsync();
                return Ok(metadata);
            }
            catch
            {
                return StatusCode(500, "An error occurred while retrieving metadata");
            }
        }

        /// <summary>
        /// Receives metadata from the client and computes the difference
        /// /// </summary>
        /// <param name="clientMetadata"></param>
        /// <returns></returns>
        [HttpPost("UploadMetadata")]
        public async Task<IActionResult> UploadMetadata([FromBody] Dictionary<string,string> clientMetadata)
        {
            _logger.LogInformation("Receiving metadata from client with {count} entries", clientMetadata.Count);
            //if the output is {}, client knows sync isn't needed
            Dictionary<string,string> diff = await _syncService.ReceiveMetadataAsync(clientMetadata);
            
            if (diff.Count > 0)
            {
                _logger.LogInformation("Found {diffCount} files that need to be synced:", diff.Count);
                foreach (var item in diff)
                {
                    _logger.LogDebug("Diff item: {key} -> {value}", item.Key, item.Value);
                }
            }
            else
            {
                _logger.LogInformation("No files need to be synced - client is up to date");
            }
            
            _memoryCache.Set("diff", diff);
            return Ok(diff);
        }


        // Currently only supports one device at a time.
        // ---
        // Problem: Two devices X and Y request to sync with the server in sequence.
        // Neither of them download as of yet. At the moment, the server
        // only stores the diff from device Y (as it is overwritten when requested).
        // So if device X requests to download before device Y does, it will
        // download device Y's files.

        // Solution 1: change this to a post method that accepts a diff record from
        // the currently requesting device instead of storing that record on the
        // server.

        // Solution 2: assign each device a signature (maybe through jwt?) that
        // the server can recognize and choose the correct record in memory
        // according to the signature.
        [HttpGet("DownloadDiff")]
        public IActionResult DownloadDiff()
        {
            _logger.LogInformation("Client requesting diff download");
            _memoryCache.TryGetValue("diff", out Dictionary<string,string>? diff);
            if (diff is null || diff.Count == 0)
            {
                _logger.LogInformation("No diff available for download - returning NoContent");
                return NoContent();
            }
            
            _logger.LogInformation("Preparing diff with {count} files for download", diff.Count);
            byte[] fileBytes = _syncService.DownloadDiff(diff);
            return File(fileBytes, "application/zip", "diff.zip");
        }

        // [HttpGet("downloadcert")]
        // public async Task<IActionResult> DownloadCert()
        // {
        //     var _certPath = "mdb.crt";
        //     if (!System.IO.File.Exists(_certPath))
        //     {
        //         return NotFound("Certificate file not found.");
        //     }

        //     var fileBytes = await System.IO.File.ReadAllBytesAsync(_certPath);
        //     var fileName = "mdb.crt";
        //     var contentType = "application/octet-stream"; // MIME type for .pfx files

        //     return File(fileBytes, contentType, fileName);
        // }

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
