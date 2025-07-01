
using Evanto.Mcp.Apps;
using Evanto.Mcp.Common.Settings;
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
            .AddSupportWizardServices();

        ConfigureLoggingForWeb(builder.Logging);

        // For SSE support, we'll use a different approach
        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithSupportWizardMcpTools();

        var app = builder.Build();

        // Test DB access
        if (!await TestDatabaseConnections(app)) return;

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

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configures logging for web application with normal output. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="logging">  The logging builder. </param>
    ///-------------------------------------------------------------------------------------------------
    private static void ConfigureLoggingForWeb(ILoggingBuilder logging)
    {
        // For web applications, we can use normal logging
        logging.AddConsole();
        logging.AddDebug();

        // Still suppress some verbose Entity Framework logging
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests database connections silently. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="app">  The application. </param>
    ///
    /// <returns>   True if all database connections are successful, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static async Task<Boolean> TestDatabaseConnections(Object app)
    {
        // Test DB access silently for MCP compatibility (no output to avoid JSON protocol interference)
        if (app is WebApplication webApp)
        {
            if (!await webApp.TestSupportWizardAccessAsync()) return false;
            // if (!await webApp.TestPrDocAccessAsync()) return false;
        }

        else if (app is IHost host)
        {
            if (!await host.TestSupportWizardAccessAsync()) return false;
            // if (!await host.TestPrDocAccessAsync()) return false;
        }

        return true;
    }

}