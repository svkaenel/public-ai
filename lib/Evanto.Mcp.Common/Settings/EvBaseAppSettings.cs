using System;

namespace Evanto.Mcp.Common.Settings;

public class EvBaseAppSettings
{
    public Boolean                  UseConsoleLogging           { get; set; } = false;
    public Boolean                  ShowThinkNodes              { get; set; } = false;
    public Boolean                  RunTests                    { get; set; } = false;
    public Boolean                  QuickTests                  { get; set; } = false;
    public Boolean                  EnableTelemetry             { get; set; } = false;
    public EvTelemetrySettings      Telemetry                   { get; set; } = new();
    public EvChatClientSettings[]   ChatClients                 { get; set; } = Array.Empty<EvChatClientSettings>();
    public EvEmbeddingSettings[]    EmbeddingProviders          { get; set; } = Array.Empty<EvEmbeddingSettings>();
    public String                   DefaultChatClient           { get; set; } = "Ionos";
    public String                   DefaultEmbeddingProvider    { get; set; } = "OllamaSharp";

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to find a chatclient provider by name. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="providerName"> The name of the provider. </param>
    ///
    /// <returns>   The provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvChatClientSettings? GetChatClient(String providerName)
    {
        return ChatClients?.FirstOrDefault(p => String.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to get the default chatclient provider. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <returns>   The default provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvChatClientSettings? GetDefaultChatClient()
    {
        return GetChatClient(DefaultChatClient);
    }
    
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to find an embedding provider by name. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <param name="providerName"> The name of the provider. </param>
    ///
    /// <returns>   The provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvEmbeddingSettings? GetEmbeddingProvider(String providerName)
    {
        return EmbeddingProviders?.FirstOrDefault(p => String.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase));
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Helper method to get the default embedding provider. </summary>
    ///
    /// <remarks>   SvK, 23.06.2025. </remarks>
    ///
    /// <returns>   The default provider configuration or null if not found. </returns>
    ///-------------------------------------------------------------------------------------------------
    public EvEmbeddingSettings? GetDefaultEmbeddingProvider()
    {
        return GetEmbeddingProvider(DefaultEmbeddingProvider);
    }

}

