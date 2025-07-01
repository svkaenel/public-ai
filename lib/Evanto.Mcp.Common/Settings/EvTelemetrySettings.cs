using System;

namespace Evanto.Mcp.Common.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration for OpenTelemetry telemetry settings. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvTelemetrySettings
{
    public Boolean              Enabled                 { get; set; } = false;
    public String               ServiceName             { get; set; } = "cmd-mcp-host";
    public String               ServiceVersion          { get; set; } = "1.0.0";
    public Boolean              EnableConsoleExporter   { get; set; } = false;
    public Boolean              EnableOtlpExporter      { get; set; } = true;
    public String               OtlpEndpoint            { get; set; } = "http://localhost:4317";
    public Boolean              LogSensitiveData        { get; set; } = false;
    public Double               SamplingRatio           { get; set; } = 1.0;
    public String[]             ActivitySources         { get; set; } = new[] { "Microsoft.Extensions.AI" };
    public Boolean              EnableLogging           { get; set; } = true;
    public Boolean              EnableLogConsoleExporter { get; set; } = false;
    public Boolean              EnableLogOtlpExporter   { get; set; } = true;
    public String?              Headers                 { get; set; } = null; // Optional headers for OTLP exporter
}