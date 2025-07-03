
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

        // Create a WebApplicationBuilder
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Application settings must be provided for web application.");
        }

        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use the specified URL
        if (!String.IsNullOrEmpty(settings.SSEListenUrls))
        {
            builder.WebHost.UseUrls(settings.SSEListenUrls);
        }

        builder
            .AddSupportWizardDB();

        builder.Services
            .AddSupportWizardServices()
            .AddSupportDocs(settings);

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithSupportWizardMcpTools()
            .WithSupportDocMcpTools();

        var app = builder.Build();

        if (settings.AutoMigrateDatabase)
        {   // Automatically migrate the database if configured
            if (!app.MigrateDatabase())
            {   // Failed to migrate the database
                logger.LogError("Failed to migrate the database. Please check your configuration.");
                // return;
            }
        }
        
        if (!await app.TestSupportWizardAccessAsync())
        {   // Test DB access failed
            logger.LogError("Failed to access the support wizard database. Please check your configuration.");
        }

        if (!await app.TestSupportDocsAccessAsync("test"))
        {   // Test DB access failed
            logger.LogError("Failed to access the support documentation database. Please check your configuration.");
        }

        app.MapMcp();

        app.MapGet("/prompts/list", () =>
        {
            return Results.Json(new { prompts = new List<Object>() });
        });

        app.MapGet("/resources/list", () =>
        {
            return Results.Json(new { resources = new List<Object>() });
        });

        await app.RunAsync();
    }

 }