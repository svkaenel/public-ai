///-------------------------------------------------------------------------------------------------
/// <summary>   Result of testing an entire MCP server. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
///
using System;
using System.Collections.Generic;

namespace Evanto.Mcp.Host.Tests;

public class EvMcpTestResult
{
    public String                       ServerName          { get; set; } = String.Empty;
    public Boolean                      Success             { get; set; }
    public TimeSpan                     TotalDuration       { get; set; }
    public IList<EvMcpToolTestResult>   ToolResults         { get; set; } = new List<EvMcpToolTestResult>();
    public String?                      ErrorMessage        { get; set; }
    
    public Int32                        SuccessfulTests     => ToolResults.Count(t => t.Success);
    public Int32                        TotalTests          => ToolResults.Count;
}
