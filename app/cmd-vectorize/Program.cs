// Program.cs
using Evanto.Mcp.Apps;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Vectorize.Extensions;
using Evanto.Mcp.Vectorize.Settings;
using Evanto.Mcp.QdrantDB.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Evanto.Mcp.Vectorize;

///-------------------------------------------------------------------------------------------------
/// <summary>   Main program class for PDF vectorization application. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class Program
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Main entry point of the application. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="args">   The command line arguments. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task Main(String[] args)
    {
        // 1. Set up configuration and logging
        var appHelper               = EvCmdAppHelper.Create;
        var settings                = appHelper.LoadConfiguration<EvVectorizeAppSettings>();
        var (logger, loggerFactory) = appHelper.GetLogger(settings);

        try
        {   // create and build host
            var host = CreateHostBuilder(args, settings);
            var app  = host.Build(); // assemble all

            using var scope         = app.Services.CreateScope();
            var   processingService = scope.ServiceProvider.GetRequiredService<IEvPdfProcessingService>();

            logger.LogInformation("Starting PDF vectorization process...");

            var result = await processingService.ProcessNewPdfsAsync();

            logger.LogInformation("Processing completed. Processed: {ProcessedCount}, Skipped: {SkippedCount}, Errors: {ErrorCount}",
                result.ProcessedCount, result.SkippedCount, result.ErrorCount);

            if (result.Errors.Any())
            {   // log errors if any occurred
                logger.LogWarning("Errors occurred during processing:");
                foreach (var error in result.Errors)
                {
                    logger.LogError("File: {File}, Error: {Error}", error.Key, error.Value);
                }
            }
        }

        catch (Exception ex)
        {   // log fatal error and exit
            logger.LogCritical(ex, "Application terminated unexpectedly");
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates the host builder with configured services. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="args">   The command line arguments. </param>
    ///
    /// <returns>   The configured host builder. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static IHostBuilder CreateHostBuilder(String[] args, EvVectorizeAppSettings settings) 
    {   // check requirements
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.Embeddings);
        ArgumentNullException.ThrowIfNull(settings.Qdrant);

        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {   // register configuration and services
                services.Configure<EvVectorizeAppSettings>(context.Configuration);
                services.AddPdfVectorizationServices();

                // Register Qdrant document repository
                services.AddQdrantDocumentRepository(settings.Qdrant);

                services.AddSingleton(settings.Embeddings);
                services.AddSingleton(settings.Qdrant);
            });
    }
}
