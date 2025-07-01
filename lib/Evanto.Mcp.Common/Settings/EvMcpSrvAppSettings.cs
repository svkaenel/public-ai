using System;

namespace Evanto.Mcp.Common.Settings;

public class EvMcpSrvAppSettings : EvBaseAppSettings
{
    public String           SystemName          { get; set; } = String.Empty;
    public Boolean          EnableSSE           { get; set; } = false;
    public String           SSEListenUrls       { get; set; } = "http://0.0.0.0:5555";
}
