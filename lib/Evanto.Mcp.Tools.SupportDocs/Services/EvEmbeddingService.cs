using System;
using System.Threading.Tasks;
using Evanto.Mcp.Tools.SupportDocs.Config;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace Evanto.Mcp.Tools.SupportDocs.Services;

/// <summary>
/// Service für die Generierung von Text-Embeddings.
/// Created: 30.05.2025
/// </summary>
public class EvEmbeddingService(IOptions<EvSupportDocSettings> config, ILogger<EvEmbeddingService> logger) : IEvEmbeddingService
{
    private readonly EvSupportDocSettings     mConfig                 = config.Value;
    private readonly ILogger<EvEmbeddingService>             mLogger                 = logger;
    private readonly OllamaApiClient                       mOllamaClient           = new(new Uri(config.Value.OllamaEndpoint)) { SelectedModel = config.Value.OllamaModel };

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generiert ein Embedding für den gegebenen Text. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="text">    Der zu verarbeitende Text. </param>
    ///
    /// <returns>   Das generierte Embedding als ReadOnlyMemory. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<ReadOnlyMemory<Single>> GenerateEmbeddingAsync(String text)
    {
        if (String.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        try
        {   // use the correct OllamaSharp method for generating embeddings
            var request = new EmbedRequest
            {
                Model = mConfig.OllamaModel,
                Input = new List<String> { text }
            };

            var response = await mOllamaClient.EmbedAsync(request);
            
            if (response?.Embeddings?.Any() == true)
            {   // get the first embedding (since we only sent one input) and convert doubles to floats
                var embedding = response.Embeddings.First().Select(d => (Single) d).ToArray();
                return new ReadOnlyMemory<Single>(embedding);
            }
            
            throw new InvalidOperationException($"No embeddings returned for text of length {text.Length}");
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to generate embedding for text of length {Length}", text.Length);
            throw new InvalidOperationException("Failed to generate embedding", ex);
        }
    }
}
