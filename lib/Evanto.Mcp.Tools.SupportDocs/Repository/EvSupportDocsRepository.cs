using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using Evanto.Mcp.Tools.SupportDocs.ViewModels;
using Evanto.Mcp.Common.Settings;

namespace Evanto.Mcp.Tools.SupportDocs.Repository;

public class EvSupportDocsRepository(
    EvQdrantSettings                    settings,
    IEvEmbeddingService                 embeddingService,
    ILogger<EvSupportDocsRepository>    logger) : IEvSupportDocsRepository, IDisposable
{
    private readonly EvQdrantSettings                   mSettings           = settings;
    private readonly IEvEmbeddingService                mEmbeddingService   = embeddingService;
    private readonly ILogger<EvSupportDocsRepository>   mLogger             = logger;
    private readonly QdrantClient                       mQdrantClient       = new(settings.QdrantEndpoint, settings.QdrantPort);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Sucht in der Produktdokumentation nach passenden Text-Chunks. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="query">    Die Suchanfrage. </param>
    /// <param name="limit">    Maximale Anzahl der Ergebnisse. </param>
    ///
    /// <returns>   Gefundene Dokumentations-Chunks mit Dateiname und Score. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<EvSupportDocViewModel>> GetSupportDocsAsync(String query, Int32 limit = 10)
    {
        if (String.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty", nameof(query));
        }

        if (limit <= 0)
        {
            limit = mSettings.SearchLimit;
        }

        try
        {   // generate embedding for query
            mLogger.LogDebug("Generating embedding for query: {Query}", query);
            var queryEmbedding = await mEmbeddingService.GenerateEmbeddingAsync(query);

            // search in vector database
            mLogger.LogDebug("Searching in collection {Collection} with limit {Limit}", 
                mSettings.VectorCollectionName, limit);
            
            var searchResult = await mQdrantClient.SearchAsync(
                mSettings.VectorCollectionName,
                queryEmbedding.ToArray(),
                limit: (UInt64) limit,
                scoreThreshold: mSettings.MinimumScore
            );

            if (searchResult == null || !searchResult.Any())
            {   // no results found
                mLogger.LogInformation("No documents found for query: {Query}", query);
                return Enumerable.Empty<EvSupportDocViewModel>();
            }

            // convert to viewmodels
            var results = searchResult.Select(CreateViewModelFromSearchResult).ToList();
            
            mLogger.LogInformation("Found {Count} documents for query: {Query}", results.Count, query);
            
            return results;
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to search product documentation for query: {Query}", query);
            throw new InvalidOperationException($"Failed to search product documentation: {ex.Message}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets the file names of all documentation chunks matching the query. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="query">    The search query. </param>
    ///
    /// <returns>   A collection of unique file names from the documentation chunks. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<String>> GetFileNames(String query, Int32 limit = 10)
    {
        var docs = await GetSupportDocsAsync(query, limit);
        if (docs == null || !docs.Any())
        {
            return Enumerable.Empty<String>();
        }

        return docs.Select(d => d.FileName).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Erstellt ViewModel aus Suchergebnis. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="result">   Das Suchergebnis von Qdrant. </param>
    ///
    /// <returns>   ProductDocumentationViewModel mit allen relevanten Daten. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static EvSupportDocViewModel CreateViewModelFromSearchResult(ScoredPoint result)
    {   // extract payload data
        var payload = result.Payload;

        return new EvSupportDocViewModel
        {
            FileName     = payload["fileName"].StringValue,
            Content      = payload["content"].StringValue,
            Score        = result.Score,
            ChunkIndex   = (Int32)payload["chunkIndex"].IntegerValue,
            TotalChunks  = (Int32)payload["totalChunks"].IntegerValue,
            ChunkId      = payload.ContainsKey("chunkId") ? payload["chunkId"].StringValue : String.Empty,
            BaseFileName = payload.ContainsKey("baseFileName") ? payload["baseFileName"].StringValue : String.Empty
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Dispose method to release resources. </summary>
    /// <remarks>   SvK, 30.05.2025. </remarks>
    /// <param name="disposing">   True to release both managed and unmanaged resources; 
    /// false to release only unmanaged resources. </param>
    ///-------------------------------------------------------------------------------------------------
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Dispose method to release resources.
    ///     This method is called by the Dispose method and can be overridden in derived classes.
    ///     If disposing is true, managed resources are disposed; if false, only unmanaged resources
    /// </summary>
    /// 
    /// <remarks>   SvK, 30.05.2025. </remarks>
    /// 
    /// <param name="disposing"></param>
    ///-------------------------------------------------------------------------------------------------
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposing)
        {   // dispose managed resources
            mQdrantClient?.Dispose();
        }
    }
}
