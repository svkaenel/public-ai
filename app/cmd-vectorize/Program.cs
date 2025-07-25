﻿// Program.cs
using Evanto.Mcp.Apps;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Vectorize.Extensions;
using Evanto.Mcp.Vectorize.Settings;
using Evanto.Mcp.QdrantDB.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Evanto.Mcp.Embeddings.Extensions;
using Evanto.Mcp.Pdfs.Extensions;

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
    {   // 1. Set up configuration and logging
        var appHelper               = EvCmdAppHelper.Create;
        var settings                = appHelper.LoadConfiguration<EvVectorizeAppSettings>();
        var (logger, loggerFactory) = appHelper.GetLogger(settings);

        try
        {   // create and build host
            var host = CreateHostBuilder(args, loggerFactory, settings);
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
    /// <param name="loggerFactory">   The logger factory to use for logging. </param>
    /// <param name="settings"> The application settings containing provider configuration. </param>
    ///
    /// <returns>   The configured host builder. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static IHostBuilder CreateHostBuilder(String[] args, ILoggerFactory loggerFactory, EvVectorizeAppSettings settings) 
    {   // check requirements
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.EmbeddingProviders);
        ArgumentNullException.ThrowIfNull(settings.Qdrant);

        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {   // register configuration and services
                services.Configure<EvVectorizeAppSettings>(context.Configuration);
                services.AddPdfVectorizationServices();

                // Register embedding service
                services.AddEmbeddings(loggerFactory, settings);

                // Register Qdrant document repository
                services.AddQdrantDocumentRepository(settings.Qdrant);

                // Register iText PDF text extractor service
                services.AddPdfTextExtractor();

                services.AddSingleton(settings.Qdrant);
            });
    }
}
