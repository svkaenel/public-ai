// See https://aka.ms/new-console-template for more information
using Evanto.Mcp.Apps;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportWizard.Extensions;

namespace Brunner.Mcp.Server;

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

         // Create a HostBuilder
        var builder  = Host.CreateApplicationBuilder(args);

        ConfigureLoggingForConsole(builder.Logging);

        builder
            .AddSupportWizardDB(settings);
            
        builder.Services
            .AddSupportWizardServices()
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithSupportWizardMcpTools();

        var app = builder.Build();

        // Test DB access
        if (!await TestDatabaseConnections(app)) return;

        await app.RunAsync();
    }

 
 
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configures common services for both web and console applications. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="services">         The service collection. </param>
    /// <param name="configuration">    The configuration. </param>
    /// <param name="settings">         The application settings. </param>
    ///-------------------------------------------------------------------------------------------------
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSupportWizardServices();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configures logging for console application to avoid STDIO interference. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="logging">  The logging builder. </param>
    ///-------------------------------------------------------------------------------------------------
    private static void ConfigureLoggingForConsole(ILoggingBuilder logging)
    {
        // Configure logging to stderr only to avoid interfering with MCP JSON-RPC communication
        logging.ClearProviders();
        logging.AddConsole(consoleLogOptions =>
        {   // configure all logs to go to stderr
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Completely suppress Entity Framework Core logging to avoid stdout interference
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Connection", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
        logging.AddFilter("Qdrant.Client", LogLevel.None);
        logging.AddFilter("Brunner.Mcp.ProductDocumentation", LogLevel.None);

        // Suppress other Microsoft logging that might interfere
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("ModelContextProtocol.Server", LogLevel.None);
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