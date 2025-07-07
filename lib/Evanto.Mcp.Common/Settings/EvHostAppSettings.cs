using System;
using System.Collections.Generic;
using System.Linq;

namespace Evanto.Mcp.Common.Settings;

///-------------------------------------------------------------------------------------------------
/// <summary>   Configuration for chat client settings. </summary>
///
/// <remarks>   SvK, 23.06.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvHostAppSettings : EvBaseAppSettings
{
    public String                       SelectedProvider    { get; private set; } = String.Empty;
    public String                       SelectedModel       { get; private set; } = String.Empty;
    public IList<EvMcpServerSettings>   McpServers          { get; set; }         = new List<EvMcpServerSettings>();
    public EvWebSettings                WebUI               { get; set; }         = new();
    public Single                       Temperature         { get; set; }         = 1.0f;
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
        SelectedProvider = String.IsNullOrEmpty(overrideProvider) ? DefaultChatClient : overrideProvider;

        var provider = GetChatClient(SelectedProvider);
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
        SelectedModel = String.IsNullOrEmpty(overrideModel) ? GetDefaultChatClient()?.DefaultModel ?? String.Empty : overrideModel;
    }
}

