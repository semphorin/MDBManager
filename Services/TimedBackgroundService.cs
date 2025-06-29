using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MDBManager.Services
{
    public class TimedBackgroundService : BackgroundService
    {
        private readonly ILogger<TimedBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromDays(1); // Run every day

        public TimedBackgroundService(ILogger<TimedBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            // Wait for the first execution (optional - you can remove this to start immediately)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the timed background service.");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task DoWorkAsync()
        {
            _logger.LogInformation("Executing timed background task at: {time}", DateTimeOffset.Now);

            // Create a scope to get scoped services
            using var scope = _serviceProvider.CreateScope();
            
            // Get the metadata service to save generated metadata to database
            var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();
            
            try
            {
                // Run your Python script to generate metadata
                await RunMetadataGenerationAsync();
                
                // Load the generated metadata.json and save it to database
                await SaveMetadataToDatabaseAsync(metadataService);
                
                _logger.LogInformation("Timed background task completed successfully at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during timed background task execution");
            }
        }

        private async Task RunMetadataGenerationAsync()
        {
            // Example: Run your Python script
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = "generateJSONMetadata.py",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = processStartInfo };
            process.Start();
            
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Metadata generation completed successfully");
            }
            else
            {
                _logger.LogError("Metadata generation failed with exit code {exitCode}. Error: {error}", 
                    process.ExitCode, error);
            }
        }

        private async Task SaveMetadataToDatabaseAsync(IMetadataService metadataService)
        {
            try
            {
                string metadataPath = Path.Combine(Directory.GetCurrentDirectory(), "metadata.json");
                
                if (!File.Exists(metadataPath))
                {
                    _logger.LogWarning("metadata.json not found, skipping database update");
                    return;
                }

                string jsonContent = await File.ReadAllTextAsync(metadataPath);
                var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                
                if (metadata != null && metadata.Any())
                {
                    await metadataService.SaveMetadataAsync(metadata);
                    _logger.LogInformation("Successfully saved {count} metadata entries to database", metadata.Count);
                }
                else
                {
                    _logger.LogWarning("No metadata found in JSON file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save metadata to database");
                throw;
            }
        }
    }
}
