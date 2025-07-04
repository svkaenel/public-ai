using Evanto.Mcp.Apps;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportWizard.Extensions;
using Evanto.Mcp.Tools.SupportDocs.Extensions;
using System.Diagnostics;

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

        logger.LogDebug($"Starting STDIO MCP Server with settings: {settings.ToJson()}");

        // Create a HostBuilder
        var builder = Host.CreateApplicationBuilder(args);

        builder
            .AddSupportWizard();

        builder.Services
            .AddSupportDocs(settings)
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithSupportWizardMcpTools()
            .WithSupportDocMcpTools();

        var app = builder.Build();
      
        if (settings.AutoMigrateDatabase)
        {   // Automatically migrate the database if configured
            if (!app.MigrateDatabase())
            {
                logger.LogError("Failed to migrate the database. Please check your configuration.");
                // return;
            }
        }
      
        if (!await app.TestSupportWizardAccessAsync())
        {   // Test DB access failed
            logger.LogError("Failed to access the database. Please check your configuration.");
        }

        if (!await app.TestSupportDocsAccessAsync("Embeddings"))
        {   // Test DB access failed
            logger.LogError("Failed to access the support documentation database. Please check your configuration.");
        }

        await app.RunAsync();
    }


}