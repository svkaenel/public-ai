///-------------------------------------------------------------------------------------------------
/// <summary>   Factory for creating MCP clients from configuration. </summary>
///
/// <remarks>   SvK, 03.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------

using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Host.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace Evanto.Mcp.Host.Factories;

public static class EvMcpClientFactory
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates MCP clients from configuration. </summary>
    ///
    /// <param name="configuration">    The configuration. </param>
    /// <param name="logger">           The logger. </param>
    ///
    /// <returns>   The list of MCP client information. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<IList<EvMcpClientInfo>> CreateMcpClientsAsync(
        EvHostAppSettings   rootConfig,
        ILogger             logger)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(rootConfig, nameof(rootConfig));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (rootConfig.McpServers == null || !rootConfig.McpServers.Any())
        {
            logger.LogWarning("⚠️ No MCP servers configured in appsettings.json");
            return new List<EvMcpClientInfo>();
        }

        var mcpClients = new List<EvMcpClientInfo>();

        foreach (var serverConfig in rootConfig.McpServers.Where(s => s.Enabled))
        {
            try
            {
                logger.LogInformation("🔌 Connecting to MCP server: {ServerName} via {TransportType}",
                    serverConfig.Name, serverConfig.TransportType);

                IMcpClient client;

                switch (serverConfig.TransportType)
                {
                    case EvMcpTransportType.STDIO:
                        client = await CreateStdioClientAsync(serverConfig, logger);
                        break;

                    case EvMcpTransportType.SSE:
                    case EvMcpTransportType.HTTP:
                        client = await CreateSseClientAsync(serverConfig, logger);
                        break;

                    default:
                        throw new NotSupportedException($"Transport type {serverConfig.TransportType} is not supported");
                }

                logger.LogInformation("✅ Connected to MCP server: {ServerName}", serverConfig.Name);

                // Get available tools
                var tools = await client.ListToolsAsync();

                logger.LogInformation("✅ {ServerName} tools: {Tools}",
                    serverConfig.Name,
                    String.Join(", ", tools.Select(t => t.Name)));

                mcpClients.Add(new EvMcpClientInfo(serverConfig.Name, client, tools));
            }

            catch (Exception ex)
            {   // Continue with other servers even if one fails
                logger.LogError(ex, "❌ Failed to connect to MCP server: {ServerName}", serverConfig.Name);
            }
        }

        logger.LogInformation("✅ Successfully connected to {Count} MCP server(s)", mcpClients.Count);

        return mcpClients;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets all tools from multiple MCP clients. </summary>
    ///
    /// <param name="mcpClients">   The MCP clients. </param>
    ///
    /// <returns>   The combined list of all tools. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IList<McpClientTool> GetAllTools(IList<EvMcpClientInfo> mcpClients)
    {
        return mcpClients.SelectMany(mcp => mcp.Tools).ToList();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Disposes all MCP clients. </summary>
    ///
    /// <param name="mcpClients">   The MCP clients. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task DisposeAllAsync(IList<EvMcpClientInfo> mcpClients)
    {
        foreach (var mcpClient in mcpClients)
        {
            try
            {
                await mcpClient.Client.DisposeAsync();
            }

            catch (Exception ex)
            {   // Log error but continue disposing other clients
                Console.Error.WriteLine($"Error disposing {mcpClient.Name}: {ex.Message}");
            }
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Finds an MCP client by tool name. </summary>
    ///
    /// <param name="mcpClients">   The MCP clients. </param>
    /// <param name="toolName">     Name of the tool. </param>
    ///
    /// <returns>   The MCP client that has the specified tool, or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IMcpClient? FindClientByToolName(IList<EvMcpClientInfo> mcpClients, String toolName)
    {
        return mcpClients.FirstOrDefault(mcp => mcp.Tools.Any(t => t.Name == toolName))?.Client;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates STDIO MCP client. </summary>
    ///
    /// <param name="serverConfig">     The server configuration. </param>
    /// <param name="logger">           The logger. </param>
    ///
    /// <returns>   The created MCP client. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static async Task<IMcpClient> CreateStdioClientAsync(EvMcpServerSettings serverConfig, ILogger logger)
    {
        var transportOptions = new StdioClientTransportOptions
        {
            Name      = serverConfig.Name,
            Command   = serverConfig.Command,
            Arguments = serverConfig.Arguments.ToArray()
        };

        var transport = new StdioClientTransport(transportOptions);

        return await ModelContextProtocol.Client.McpClientFactory.CreateAsync(transport);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Creates SSE MCP client. </summary>
    ///
    /// <param name="serverConfig">     The server configuration. </param>
    /// <param name="logger">           The logger. </param>
    ///
    /// <returns>   The created MCP client. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static async Task<IMcpClient> CreateSseClientAsync(EvMcpServerSettings serverConfig, ILogger logger)
    {
        if (String.IsNullOrEmpty(serverConfig.Url))
        {
            throw new ArgumentException($"URL is required for SSE transport in server '{serverConfig.Name}'");
        }

        var transportOptions = new SseClientTransportOptions
        {
            Name     = serverConfig.Name,
            Endpoint = new Uri(serverConfig.Url)
        };

        var transport = new SseClientTransport(transportOptions);

        return await ModelContextProtocol.Client.McpClientFactory.CreateAsync(transport);
    }

}
