///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration class for MCP servers. </summary>
///
/// <remarks>   SvK, 03.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
///
using System;
using System.Collections.Generic;

namespace Evanto.Mcp.Common.Settings;

public class EvMcpServerSettings
{
    public String                       Name            { get; set; } = String.Empty;
    public String                       Command         { get; set; } = String.Empty;
    public IList<String>                Arguments       { get; set; } = new List<String>();
    public Boolean                      Enabled         { get; set; } = true;
    public Int32                        TimeoutSeconds  { get; set; } = 30;
    public EvMcpTransportType             TransportType   { get; set; } = EvMcpTransportType.STDIO;
    public String                       Url             { get; set; } = String.Empty;
    public IList<EvMcpToolTestSettings>   ToolTests       { get; set; } = new List<EvMcpToolTestSettings>();
}
