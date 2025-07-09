
using System.Diagnostics;
using Evanto.Mcp.Apps;
using Evanto.Mcp.Apps.Extensions;
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

        await logger.LogOutput("Starting SSE MCP Server with settings: {0}", settings.ToJson());

        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use the specified URL
        if (!String.IsNullOrEmpty(settings.SSEListenUrls))
        {
            builder.WebHost.UseUrls(settings.SSEListenUrls);
            Console.WriteLine($"Listening now on URL: {settings.SSEListenUrls}");
        }

        builder
            .AddSupportWizard();

        await logger.LogOutput($"Support Wizard added...");

        builder.Services
            .AddSupportDocs(loggerFactory, settings)
            .AddMcpServer()
            .WithHttpTransport()
            .WithSupportWizardMcpTools()
            .WithSupportDocMcpTools();

        var app = builder.Build();

        await logger.LogOutput($"DI setup finished...");

        if (settings.AutoMigrateDatabase)
        {   // Automatically migrate the database if configured
            if (!app.MigrateDatabase())
            {   // Failed to migrate the database
                await logger.LogOutput("Failed to migrate the database. Please check your configuration.");
                return;
            }

            else
            {
                await logger.LogOutput("Database migration completed successfully.");
            }
        }

        else
        {
            await logger.LogOutput("Database migration is disabled. Please ensure your database is up to date.");
        }

        if (!await app.TestSupportWizardAccessAsync())
        {   // Test DB access failed
            logger.LogError("Failed to access the support wizard database. Please check your configuration.");
        }

        if (!await app.TestSupportDocsAccessAsync("Embeddings"))
        {   // Test DB access failed
            logger.LogError("Failed to access the support documentation database. Please check your configuration.");
        }
        
        await logger.LogOutput("Tests for SSE MCP Server successful...");

        app.MapMcp();

        app.MapGet("/prompts/list", () =>
        {
            return Results.Json(new { prompts = new List<Object>() });
        });

        app.MapGet("/resources/list", () =>
        {
            return Results.Json(new { resources = new List<Object>() });
        });

        await logger.LogOutput("SSE MCP Server is finally running...");

        await app.RunAsync();
    }

 }