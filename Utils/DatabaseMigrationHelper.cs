using Microsoft.EntityFrameworkCore;
using MDBManager.Data;
using MDBManager.Services;
using System.Text.Json;

namespace MDBManager.Utils
{
    public static class DatabaseMigrationHelper
    {
        public static async Task MigrateJsonToDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MDBContext>();
            var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseMigration");
            
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "metadata.json");
            
            if (!File.Exists(jsonPath))
            {
                logger.LogInformation("No metadata.json file found, skipping migration");
                return;
            }

            try
            {
                // Check if database already has data
                var existingCount = await context.MusicMetadata.CountAsync();
                if (existingCount > 0)
                {
                    logger.LogInformation("Database already contains {count} metadata entries. Skipping migration", existingCount);
                    return;
                }

                // Read and parse JSON
                string jsonContent = await File.ReadAllTextAsync(jsonPath);
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (metadata != null && metadata.Any())
                {
                    await metadataService.SaveMetadataAsync(metadata);
                    logger.LogInformation("Successfully migrated {count} metadata entries to database", metadata.Count);
                    
                    // Optionally backup the JSON file
                    string backupPath = jsonPath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Copy(jsonPath, backupPath);
                    logger.LogInformation("JSON file backed up to: {backupPath}", backupPath);
                }
                else
                {
                    logger.LogWarning("No metadata found in JSON file");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during migration: {message}", ex.Message);
                throw;
            }
        }
    }
}
