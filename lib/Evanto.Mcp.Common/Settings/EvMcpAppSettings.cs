namespace Evanto.Mcp.Common.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Application settings for Brunner MCP server. </summary>
///
/// <remarks>   SvK, 03.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvMcpAppSettings
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Member variables. </summary>
    ///-------------------------------------------------------------------------------------------------
    public String           SystemName          { get; set; } = String.Empty;
    public Boolean          EnableSSE           { get; set; } = false;
    public String           SSEListenUrls       { get; set; } = "http://0.0.0.0:5555";
}
