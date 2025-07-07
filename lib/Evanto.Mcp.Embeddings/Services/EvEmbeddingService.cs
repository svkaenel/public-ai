using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;

namespace Evanto.Mcp.Embeddings.Services;

public class EvEmbeddingService(EvEmbeddingSettings settings, ILogger<EvEmbeddingService> logger, IEmbeddingGenerator<String, Embedding<Single>> embeddingGenerator) : IEvEmbeddingService
{
    private readonly EvEmbeddingSettings                            mSettings           = settings;
    private readonly IEmbeddingGenerator<String, Embedding<Single>> mEmbeddingGenerator = embeddingGenerator;
    private readonly ILogger<EvEmbeddingService>                    mLogger             = logger;

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
            var response = await mEmbeddingGenerator.GenerateAsync(text, new EmbeddingGenerationOptions
            {
                Dimensions  = mSettings.EmbeddingDimensions,
                ModelId     = mSettings.DefaultModel
            });

            if (response?.Vector != null)
            {   // get the first embedding (since we only sent one input) and convert doubles to Singles
                return response.Vector;
            }

            throw new InvalidOperationException($"No embeddings returned for text of length {text.Length}");
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to generate embedding for text of length {Length}", text.Length);
            throw new InvalidOperationException("Failed to generate embedding", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generate embeddings asynchronously for multiple texts. </summary>
    ///
    /// <remarks>   SvK, 26.05.2025. </remarks>
    ///
    /// <param name="texts">   The collection of texts to generate embeddings for. </param>
    ///
    /// <returns>   A list of ReadOnlyMemory of Single representing the embedding vectors. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<List<ReadOnlyMemory<Single>>> GenerateEmbeddingsAsync(IEnumerable<String> texts)
    {   // generate embeddings for multiple texts
        var embeddings = new List<ReadOnlyMemory<Single>>();
        
        foreach (var text in texts)
        {
            try
            {   // generate embedding for current text
                var embedding = await GenerateEmbeddingAsync(text);
                embeddings.Add(embedding);
                
                // add small delay to avoid overwhelming the API
                await Task.Delay(50);
            }

            catch (Exception ex)
            {   // log error and rethrow
                mLogger.LogError(ex, "Failed to generate embedding for text chunk");
                throw;
            }
        }
        
        return embeddings;
    }

}
