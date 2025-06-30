///-------------------------------------------------------------------------------------------------
/// <summary>   Result of testing a single MCP tool. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
///
using System;
using System.Collections.Generic;

namespace Evanto.Mcp.Host.Tests;

public class EvMcpToolTestResult
{
    public String                       ToolName        { get; set; } = String.Empty;
    public Boolean                      Success         { get; set; }
    public TimeSpan                     Duration        { get; set; }
    public Dictionary<String, Object?>  UsedParameters  { get; set; } = new();
    public String?                      Response        { get; set; }
    public String?                      ErrorMessage    { get; set; }
}
