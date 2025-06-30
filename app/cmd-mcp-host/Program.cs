using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ConsoleMarkdownRenderer;
using System.Text.Json;
using Evanto.Mcp.CommandLineHost.Helper;
using Evanto.Mcp.Host.Factories;
using Evanto.Mcp.Host.Tests;

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
    {   // Check for help parameter
        if (args.Contains("--help") || args.Contains("-h"))
        {
            AppHelper.ShowHelp();
            return;
        }

        // 1. Set up configuration and logging
        var rootConfig              = AppHelper.LoadConfiguration(args);
        var (logger, loggerFactory) = AppHelper.GetLogger(rootConfig);

        if (args.Contains("--list"))
        {   // List available providers and exit
            AppHelper.ShowAvailableProviders(rootConfig);
            return;
        }

        try
        {   // 2. Create MCP clients from configuration
            var mcpClients = await EvMcpClientFactory.CreateMcpClientsAsync(rootConfig, logger);

            if (!mcpClients.Any())
            {
                logger.LogWarning("⚠️ No MCP servers available - continuing with chat client only");
            }

            // Get all tools from all MCP clients
            var allTools = EvMcpClientFactory.GetAllTools(mcpClients);

            logger.LogInformation($"✅ Total available tools: {allTools.Count}");

            // 3. Test MCP tools directly (only if --test parameter is provided)
            if (rootConfig.RunTests && mcpClients.Any())
            {
                await EvMcpServerTester.TestAllMcpServersAsync(logger, rootConfig, mcpClients);
            }

            // 4. Chat Client Integration with configurable Provider
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
                await AppHelper.StartInteractiveChatLoopAsync(logger, chatClient, allTools, rootConfig);
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
    }
}