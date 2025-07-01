using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Evanto.Mcp.Tools.SupportDocs.Config;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using Evanto.Mcp.Tools.SupportDocs.ViewModels;

namespace Evanto.Mcp.Tools.SupportDocs.Repository;

/// <summary>
/// Repository für Produktdokumentation mit Vektordatenbank-Abfrage.
/// Created: 30.05.2025
/// </summary>
public class EvSupportDocsRepository(
    IOptions<EvSupportDocSettings>         config,
    IEvEmbeddingService                                   embeddingService,
    ILogger<EvSupportDocsRepository>             logger) : IEvSupportDocsRepository, IDisposable
{
    private readonly EvSupportDocSettings          mConfig             = config.Value;
    private readonly IEvEmbeddingService                          mEmbeddingService   = embeddingService;
    private readonly ILogger<EvSupportDocsRepository>    mLogger             = logger;
    private readonly QdrantClient                               mQdrantClient       = new(config.Value.QdrantEndpoint, 6334);

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
    public async Task<IEnumerable<EvSupportDocViewModel>> GetProductDocumentationAsync(String query, Int32 limit = 10)
    {
        if (String.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty", nameof(query));
        }

        if (limit <= 0)
        {
            limit = mConfig.SearchLimit;
        }

        try
        {   // generate embedding for query
            mLogger.LogDebug("Generating embedding for query: {Query}", query);
            var queryEmbedding = await mEmbeddingService.GenerateEmbeddingAsync(query);

            // search in vector database
            mLogger.LogDebug("Searching in collection {Collection} with limit {Limit}", 
                mConfig.VectorCollectionName, limit);
            
            var searchResult = await mQdrantClient.SearchAsync(
                mConfig.VectorCollectionName,
                queryEmbedding.ToArray(),
                limit: (UInt64)limit,
                scoreThreshold: mConfig.MinimumScore
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
        var docs = await GetProductDocumentationAsync(query, limit);
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
            FileName = payload["fileName"].StringValue,
            Content = payload["content"].StringValue,
            Score = result.Score,
            ChunkIndex = (Int32)payload["chunkIndex"].IntegerValue,
            TotalChunks = (Int32)payload["totalChunks"].IntegerValue,
            ChunkId = payload.ContainsKey("chunkId") ? payload["chunkId"].StringValue : String.Empty,
            BaseFileName = payload.ContainsKey("baseFileName") ? payload["baseFileName"].StringValue : String.Empty
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean disposing)
    {
        if (disposing)
        {   // dispose managed resources
            mQdrantClient?.Dispose();
        }
    }

    Task<IEnumerable<EvSupportDocViewModel>> IEvSupportDocsRepository.GetProductDocumentationAsync(string query, int limit)
    {
        throw new NotImplementedException();
    }
}
