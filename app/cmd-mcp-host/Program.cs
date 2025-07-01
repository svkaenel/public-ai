using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Evanto.Mcp.Host.Factories;
using Evanto.Mcp.Host.Tests;
using Evanto.Mcp.Common.Settings;
using OpenTelemetry.Trace;
using Evanto.Mcp.Apps;

namespace Evanto.Mcp.CommandLineHost;

public class Program
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Main entry point for the application. </summary>
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// 
    /// <param name="args">  The command line arguments. </param>
    /// 
    /// <returns>   A Task that represents the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    static async Task Main(String[] args)
    {   
        // 1. Set up configuration and logging
        var appHelper               = EvCmdAppHelper.Create;
        var rootConfig              = appHelper.LoadConfiguration<EvHostAppSettings>(args);
        var (logger, loggerFactory) = appHelper.GetLogger(rootConfig);

        if (args.Contains("--help") || args.Contains("-h"))
        {   // Check for help parameter
            appHelper.ShowHelp();
            return;
        }

        // 2. Configure OpenTelemetry if enabled (either from config or command line)
        TracerProvider? tracerProvider = null;

        var telemetryEnabled = rootConfig.Telemetry.Enabled || rootConfig.EnableTelemetry;
        if (telemetryEnabled)
        {   // override telemetry enabled flag if command line option was used
            if (rootConfig.EnableTelemetry)
            {
                rootConfig.Telemetry.Enabled = true;
            }

            tracerProvider = appHelper.ConfigureOpenTelemetry(rootConfig, logger);
        }

        if (args.Contains("--list"))
        {   // List available providers and exit
            appHelper.ShowAvailableProviders(rootConfig);
            return;
        }

        try
        {   // 3. Create MCP clients from configuration
            var mcpClients = await EvMcpClientFactory.CreateMcpClientsAsync(rootConfig, logger);

            if (!mcpClients.Any())
            {
                logger.LogWarning("⚠️ No MCP servers available - continuing with chat client only");
            }

            // Get all tools from all MCP clients
            var allTools = EvMcpClientFactory.GetAllTools(mcpClients);

            logger.LogInformation($"✅ Total available tools: {allTools.Count}");

            // 4. Test MCP tools directly (only if --test parameter is provided)
            if (rootConfig.RunTests && mcpClients.Any())
            {
                await EvMcpServerTester.TestAllMcpServersAsync(logger, rootConfig, mcpClients);
            }

            // 5. Chat Client Integration with configurable Provider
            logger.LogInformation("🤖 Creating chat client provider from configuration...");

            try
            {   // Create chat client factory
                var chatClientFactory = EvChatClientFactory.Create(loggerFactory);

                logger.LogInformation("🔗 Creating chat client using {ProviderName}...", rootConfig.SelectedProvider);

                var chatClient = chatClientFactory.CreateChatClient(rootConfig);

                if (rootConfig.RunTests)
                {   // Only test connection if --test parameter is provided
                    var connectionTest = await chatClientFactory.TestConnectionAsync(chatClient);

                    if (!connectionTest)
                    {
                        logger.LogWarning("⚠️ Chat client connection test failed, but continuing anyway...");
                    }
                }

                // Start interactive chat loop
                await appHelper.StartInteractiveChatLoopAsync(logger, chatClient, allTools, rootConfig);
            }

            catch (Exception chatClientEx)
            {
                logger.LogError(chatClientEx, "❌ Failed to create or use chat client");
                Console.WriteLine($"❌ Chat Client Error: {chatClientEx.Message}");
                Console.WriteLine($"💡 Please check your configuration in appsettings.json");
                Console.WriteLine($"💡 Make sure the configured chat service is running and accessible");
            }

            // Clean up all MCP clients
            await EvMcpClientFactory.DisposeAllAsync(mcpClients);
            logger.LogInformation("✅ All tests completed successfully!");
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while running the MCP client");
            Console.WriteLine($"Error: {ex.Message}");
        }

        finally
        {   // Clean up OpenTelemetry resources
            tracerProvider?.Dispose();
        }
    }

}