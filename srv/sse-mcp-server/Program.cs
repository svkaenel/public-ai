
using System.Diagnostics;
using Evanto.Mcp.Apps;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportDocs.Extensions;
using Evanto.Mcp.Tools.SupportWizard.Extensions;

public class Program
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary> The main entry point for this application. </summary>
    /// 
    /// <param name="args"> The command line arguments. </param>
    ///-------------------------------------------------------------------------------------------------
    public async static Task Main(String[] args)
    {
        var appHelper               = EvSrvAppHelper.Create;
        var settings                = appHelper.LoadConfiguration<EvMcpSrvAppSettings>();
        var (logger, loggerFactory) = appHelper.GetLogger(settings);

        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Application settings must be provided for web application.");
        }

        logger.LogInformation("Starting SSE MCP Server with settings: {0}", settings.ToJson());

        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use the specified URL
        if (!String.IsNullOrEmpty(settings.SSEListenUrls))
        {
            builder.WebHost.UseUrls(settings.SSEListenUrls);
            Console.WriteLine($"Listening now on URL: {settings.SSEListenUrls}");
        }

        builder
            .AddSupportWizard();

        logger.LogInformation($"Support Wizard added...");

        builder.Services
            .AddSupportDocs(loggerFactory, settings)
            .AddMcpServer()
            .WithHttpTransport()
            .WithSupportWizardMcpTools()
            .WithSupportDocMcpTools();

        var app = builder.Build();

        logger.LogInformation($"DI setup finished...");

        if (settings.AutoMigrateDatabase)
        {   // Automatically migrate the database if configured
            if (!app.MigrateDatabase())
            {   // Failed to migrate the database
                logger.LogInformation("Failed to migrate the database. Please check your configuration.");
                return;
            }

            else
            {
                logger.LogInformation("Database migration completed successfully.");
            }
        }

        else
        {
            logger.LogInformation("Database migration is disabled. Please ensure your database is up to date.");
        }

        if (!await app.TestSupportWizardAccessAsync())
        {   // Test DB access failed
            logger.LogError("Failed to access the support wizard database. Please check your configuration.");
        }

        if (!await app.TestSupportDocsAccessAsync("Embeddings"))
        {   // Test DB access failed
            logger.LogError("Failed to access the support documentation database. Please check your configuration.");
        }
        
        logger.LogInformation("Tests for SSE MCP Server successful...");

        app.MapMcp();

        app.MapGet("/prompts/list", () =>
        {
            return Results.Json(new { prompts = new List<Object>() });
        });

        app.MapGet("/resources/list", () =>
        {
            return Results.Json(new { resources = new List<Object>() });
        });

        logger.LogInformation("SSE MCP Server is finally running...");

        await app.RunAsync();
    }

 }