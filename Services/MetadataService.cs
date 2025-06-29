using Microsoft.EntityFrameworkCore;
using MDBManager.Data;
using MDBManager.Models;

namespace MDBManager.Services
{
    public interface IMetadataService
    {
        Task<Dictionary<string, string>> GetAllMetadataAsync();
        Task<MusicMetadata?> GetMetadataAsync(string filePath);
        Task SaveMetadataAsync(Dictionary<string, string> metadata);
        Task SaveMetadataAsync(string filePath, string hash, DateTime lastModified, long fileSize);
        Task DeleteMetadataAsync(string filePath);
        Task<Dictionary<string, string>> GetMetadataDiffAsync(Dictionary<string, string> clientMetadata);
        Task ClearAllMetadataAsync();
    }

    public class MetadataService : IMetadataService
    {
        private readonly MDBContext _context;
        private readonly ILogger<MetadataService> _logger;

        public MetadataService(MDBContext context, ILogger<MetadataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> GetAllMetadataAsync()
        {
            try
            {
                var metadata = await _context.MusicMetadata
                    .AsNoTracking()
                    .ToDictionaryAsync(m => m.FilePath, m => m.Hash);
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve metadata from database");
                return new Dictionary<string, string>();
            }
        }

        public async Task<MusicMetadata?> GetMetadataAsync(string filePath)
        {
            try
            {
                return await _context.MusicMetadata
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.FilePath == filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve metadata for file: {filePath}", filePath);
                return null;
            }
        }

        public async Task SaveMetadataAsync(Dictionary<string, string> metadata)
        {
            try
            {
                var existingPaths = (await _context.MusicMetadata
                    .Select(m => m.FilePath)
                    .ToListAsync()).ToHashSet();

                var metadataEntities = metadata.Select(kvp => new MusicMetadata
                {
                    FilePath = kvp.Key,
                    Hash = kvp.Value,
                    LastModified = DateTime.UtcNow,
                    FileSize = 0 // You might want to get actual file size
                }).ToList();

                // Remove entries that no longer exist
                var pathsToRemove = existingPaths.Except(metadata.Keys).ToList();
                if (pathsToRemove.Any())
                {
                    var entitiesToRemove = await _context.MusicMetadata
                        .Where(m => pathsToRemove.Contains(m.FilePath))
                        .ToListAsync();
                    _context.MusicMetadata.RemoveRange(entitiesToRemove);
                }

                // Update or add new entries
                foreach (var entity in metadataEntities)
                {
                    var existing = await _context.MusicMetadata
                        .FirstOrDefaultAsync(m => m.FilePath == entity.FilePath);
                    
                    if (existing != null)
                    {
                        existing.Hash = entity.Hash;
                        existing.LastModified = entity.LastModified;
                        _context.MusicMetadata.Update(existing);
                    }
                    else
                    {
                        await _context.MusicMetadata.AddAsync(entity);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved {count} metadata entries to database", metadata.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save metadata to database");
                throw;
            }
        }

        public async Task SaveMetadataAsync(string filePath, string hash, DateTime lastModified, long fileSize)
        {
            try
            {
                var existing = await _context.MusicMetadata
                    .FirstOrDefaultAsync(m => m.FilePath == filePath);

                if (existing != null)
                {
                    existing.Hash = hash;
                    existing.LastModified = lastModified;
                    existing.FileSize = fileSize;
                    _context.MusicMetadata.Update(existing);
                }
                else
                {
                    var metadata = new MusicMetadata
                    {
                        FilePath = filePath,
                        Hash = hash,
                        LastModified = lastModified,
                        FileSize = fileSize
                    };
                    await _context.MusicMetadata.AddAsync(metadata);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save metadata for file: {filePath}", filePath);
                throw;
            }
        }

        public async Task DeleteMetadataAsync(string filePath)
        {
            try
            {
                var metadata = await _context.MusicMetadata
                    .FirstOrDefaultAsync(m => m.FilePath == filePath);
                
                if (metadata != null)
                {
                    _context.MusicMetadata.Remove(metadata);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete metadata for file: {filePath}", filePath);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetMetadataDiffAsync(Dictionary<string, string> clientMetadata)
        {
            try
            {
                var serverMetadata = await GetAllMetadataAsync();
                var diff = new Dictionary<string, string>();

                foreach (var kvp in serverMetadata)
                {
                    if (!clientMetadata.ContainsKey(kvp.Key) || clientMetadata[kvp.Key] != kvp.Value)
                    {
                        diff[kvp.Key] = kvp.Value;
                    }
                }

                return diff;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate metadata diff");
                return new Dictionary<string, string>();
            }
        }

        public async Task ClearAllMetadataAsync()
        {
            try
            {
                await _context.MusicMetadata.ExecuteDeleteAsync();
                _logger.LogInformation("Cleared all metadata from database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear metadata from database");
                throw;
            }
        }
    }
}
