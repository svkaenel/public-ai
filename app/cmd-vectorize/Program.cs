// Program.cs
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Vectorize.Extensions;
using Evanto.Mcp.Vectorize.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Evanto.Mcp.Vectorize;

public class Program
{
    public static async Task Main(String[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();
            
            using var scope = host.Services.CreateScope();
            var processingService = scope.ServiceProvider.GetRequiredService<IEvPdfProcessingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Starting PDF vectorization process...");
            
            var result = await processingService.ProcessNewPdfsAsync();
            
            logger.LogInformation("Processing completed. Processed: {ProcessedCount}, Skipped: {SkippedCount}, Errors: {ErrorCount}", 
                result.ProcessedCount, result.SkippedCount, result.ErrorCount);
                
            if (result.Errors.Any())
            {
                logger.LogWarning("Errors occurred during processing:");
                foreach (var error in result.Errors)
                {
                    logger.LogError("File: {File}, Error: {Error}", error.Key, error.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(String[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                services.Configure<EvVectorizeAppSettings>(context.Configuration);
                services.AddPdfVectorizationServices();
            });
}
