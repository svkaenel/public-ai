using System;

namespace Evanto.Mcp.Common.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Basis-Konfiguration für einen Chat-Client-Provider. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvChatClientSettings
{
    public String       ProviderName            { get; set; } = String.Empty;
    public String       Endpoint                { get; set; } = String.Empty;
    public String       ApiKey                  { get; set; } = String.Empty;
    public String       DefaultModel            { get; set; } = String.Empty;
    public String[]     AvailableModels         { get; set; } = Array.Empty<String>();
}
