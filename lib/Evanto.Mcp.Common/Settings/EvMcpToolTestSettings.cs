///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration for testing individual MCP tools. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Evanto.Mcp.Common.Settings;

public class EvMcpToolTestSettings
{
    public String                       ToolName        { get; set; } = String.Empty;
    public Dictionary<String, Object?>  TestParameters  { get; set; } = new();
    public Boolean                      Enabled         { get; set; } = true;
    public Int32                        TimeoutSeconds  { get; set; } = 30;
}
