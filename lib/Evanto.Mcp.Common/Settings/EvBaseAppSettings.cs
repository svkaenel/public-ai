using System;

namespace Evanto.Mcp.Common.Settings;

public class EvBaseAppSettings
{
    public Boolean                      UseConsoleLogging   { get; set; }         = false;
    public Boolean                      ShowThinkNodes      { get; set; }         = false;
    public Boolean                      RunTests            { get; set; }         = false;
    public Boolean                      QuickTests          { get; set; }         = false;
    public Boolean                      EnableTelemetry     { get; set; }         = false;
    public EvTelemetrySettings          Telemetry           { get; set; }         = new();
}
