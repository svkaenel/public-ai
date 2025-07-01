using System;
using System.Collections.Generic;
using System.Linq;

namespace Evanto.Mcp.Common.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration for chat client settings. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvHostSettings
{
    public Boolean                      UseConsoleLogging   { get; set; }         = false;
    public Boolean                      ShowThinkNodes      { get; set; }         = false;
    public Boolean                      RunTests            { get; set; }         = false;
    public Boolean                      QuickTests          { get; set; }         = false;
    public Boolean                      EnableTelemetry     { get; set; }         = false;
    public String                       SelectedProvider    { get; private set; } = String.Empty;
    public String                       SelectedModel       { get; private set; } = String.Empty;
    public String                       DefaultProvider     { get; set; }         = "Ionos";
    public EvChatClientSettings[]       Providers           { get; set; }         = Array.Empty<EvChatClientSettings>();
    public IList<EvMcpServerSettings>   McpServers          { get; set; }         = new List<EvMcpServerSettings>();
    public EvWebSettings                WebUI               { get; set; }         = new();
    public EvTelemetrySettings          Telemetry           { get; set; }         = new();
    public float                        Temperature         { get; set; }         = 0.5f;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to find a provider by name. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="providerName"> The name of the provider. </param>
    ///
    /// <returns>   The provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvChatClientSettings? GetProvider(String providerName)
    {
        return Providers?.FirstOrDefault(p => String.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to get the default provider. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <returns>   The default provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvChatClientSettings? GetDefaultProvider()
    {
        return GetProvider(DefaultProvider);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Setzt den ausgewählten Provider. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// <param name="overrideProvider"> Der Name des zu verwendenden Providers. </param>
    ///
    /// <exception cref="InvalidOperationException"> Wenn der angegebene Provider nicht in der Konfiguration gefunden wurde. </exception>
    ///-------------------------------------------------------------------------------------------------    
    public void SetSelectedProvider(String? overrideProvider)
    {
        SelectedProvider = String.IsNullOrEmpty(overrideProvider) ? DefaultProvider : overrideProvider;

        var provider = GetProvider(SelectedProvider);
        if (provider == null)
            throw new InvalidOperationException($"Provider '{SelectedProvider}' not found in configuration.");
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Setzt das ausgewählte Modell. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    /// 
    /// <param name="overrideModel"> Der Name des zu verwendenden Modells. </param>
    ///-------------------------------------------------------------------------------------------------
    public void SetSelectedModel(String? overrideModel)
    {
        SelectedModel = String.IsNullOrEmpty(overrideModel) ? GetDefaultProvider()?.DefaultModel ?? String.Empty : overrideModel;
    }
}

