using ModelContextProtocol.Client;

namespace Evanto.Mcp.Host.Models;

///-------------------------------------------------------------------------------------------------
/// <summary>   Information describing the MCP client. </summary>
///
/// <remarks>   SvK, 03.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public record EvMcpClientInfo(String Name, IMcpClient Client, IList<McpClientTool> Tools);

