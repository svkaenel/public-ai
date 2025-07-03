using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Vectorize.Models;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Globalization;

namespace Evanto.Mcp.Vectorize.Services;

///-------------------------------------------------------------------------------------------------
/// <summary>   Vector store service implementation using Qdrant. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvVectorStoreService(EvQdrantSettings settings, ILogger<EvVectorStoreService> logger) : IEvVectorStoreService, IDisposable
{
    private readonly QdrantClient                           mQdrantClient       = new QdrantClient(settings?.QdrantEndpoint ?? throw new ArgumentNullException(nameof(settings)), settings?.QdrantPort ?? 6334);
    private readonly EvQdrantSettings                       mSettings           = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<EvVectorStoreService>          mLogger             = logger ?? throw new ArgumentNullException(nameof(logger));
    private          Boolean                                mDisposed           = false;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initialize vector store asynchronously. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task InitializeAsync()
    {   // initialize the vector store and create collection if needed
        try
        {   // check if collection exists
            var collections      = await mQdrantClient.ListCollectionsAsync();
            var collectionExists = collections.Any(c => c == mSettings.VectorCollectionName);
            
            if (!collectionExists)
            {   // create new collection
                mLogger.LogInformation("Creating collection: {CollectionName}", mSettings.VectorCollectionName);
                
                await mQdrantClient.CreateCollectionAsync(mSettings.VectorCollectionName, new VectorParams
                {
                    Size     = mSettings.VectorDimension,
                    Distance = Distance.Cosine
                });
                
                mLogger.LogInformation("Successfully created collection: {CollectionName}", mSettings.VectorCollectionName);
            }

            else
            {   // collection already exists
                mLogger.LogDebug("Collection already exists: {CollectionName}", mSettings.VectorCollectionName);
            }

        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to initialize vector store");
            throw new InvalidOperationException("Vector store initialization failed", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Check if document exists asynchronously. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="documentId">   Identifier for the document. </param>
    ///
    /// <returns>   True if document exists, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> DocumentExistsAsync(String documentId)
    {   // check if document exists in vector store
        if (String.IsNullOrWhiteSpace(documentId))
        {   // invalid document id
            mLogger.LogWarning("DocumentExistsAsync called with null or empty documentId");
            return false;
        }

        try
        {   // check document existence
            var pointIds = new List<PointId> { new PointId { Uuid = documentId } };
            var response = await mQdrantClient.RetrieveAsync(mSettings.VectorCollectionName, pointIds);
            
            return response.Any();
        }

        catch (Exception ex)
        {   // log warning and return false
            mLogger.LogWarning(ex, "Failed to check document existence for: {DocumentId}", documentId);
            return false;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Store document asynchronously. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="document">   The document to store. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task StoreDocumentAsync(EvDocumentRecord document)
    {   // store single document in vector store
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        
        if (String.IsNullOrWhiteSpace(document.Id))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(document));

        try
        {   // create point and store
            var point = CreatePointFromDocument(document);
            await mQdrantClient.UpsertAsync(mSettings.VectorCollectionName, new[] { point });
            
            mLogger.LogDebug("Stored document chunk: {DocumentId}", document.Id);
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to store document: {DocumentId}", document.Id);
            throw new InvalidOperationException($"Failed to store document {document.Id}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Store multiple documents asynchronously. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="documents">   The documents to store. </param>
    ///
    /// <returns>   A Task representing the asynchronous operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task StoreDocumentsAsync(IEnumerable<EvDocumentRecord> documents)
    {   // store multiple documents in vector store
        if (documents == null)
            throw new ArgumentNullException(nameof(documents));

        var documentList = documents.ToList();
        if (!documentList.Any())
        {   // empty collection
            mLogger.LogWarning("StoreDocumentsAsync called with empty document collection");
            return;
        }

        try
        {   // create points and store batch
            var points = documentList.Select(CreatePointFromDocument).ToList();
            await mQdrantClient.UpsertAsync(mSettings.VectorCollectionName, points);
            
            mLogger.LogInformation("Stored {Count} document chunks", points.Count);
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to store documents batch of {Count} items", documentList.Count);
            throw new InvalidOperationException("Failed to store document batch", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Search similar documents asynchronously. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="queryVector">   The query vector. </param>
    /// <param name="limit">         The maximum number of results. </param>
    ///
    /// <returns>   An enumerable of similar documents. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<EvDocumentRecord>> SearchSimilarAsync(ReadOnlyMemory<Single> queryVector, Int32 limit = 10)
    {   // search for similar documents
        if (queryVector.IsEmpty)
            throw new ArgumentException("Query vector cannot be empty", nameof(queryVector));
        
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive", nameof(limit));

        try
        {   // perform vector search
            var searchResult = await mQdrantClient.SearchAsync(
                mSettings.VectorCollectionName,
                queryVector.ToArray(),
                limit: (UInt64)limit
            );

            if (searchResult == null || !searchResult.Any())
            {   // no results found
                mLogger.LogDebug("Search returned no results");
                return Enumerable.Empty<EvDocumentRecord>();
            }

            return searchResult.Select(CreateDocumentFromSearchResult).ToList();
        }

        catch (Exception ex)
        {   // log error and rethrow
            mLogger.LogError(ex, "Failed to search similar documents with limit {Limit}", limit);
            throw new InvalidOperationException("Failed to search similar documents", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Create point from document for vector storage. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="document">   The document to convert. </param>
    ///
    /// <returns>   A PointStruct for vector storage. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static PointStruct CreatePointFromDocument(EvDocumentRecord document)
    {   // create point structure for qdrant
        return new PointStruct
        {
            Id      = new PointId { Uuid = document.Id },
            Vectors = document.Vector.ToArray(),
            Payload =
            {
                ["fileName"]     = document.FileName,
                ["content"]      = document.Content,
                ["processedAt"]  = document.ProcessedAt.ToString("O", CultureInfo.InvariantCulture),
                ["fileHash"]     = document.FileHash,
                ["chunkIndex"]   = document.ChunkIndex,
                ["totalChunks"]  = document.TotalChunks,
                ["baseFileName"] = Path.GetFileNameWithoutExtension(document.FileName), // For easier searching
                ["chunkId"]      = $"{Path.GetFileNameWithoutExtension(document.FileName)}_{document.ChunkIndex}" // Legacy ID format for reference
            }
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Create document from search result. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="result">   The search result to convert. </param>
    ///
    /// <returns>   A DocumentRecord from search result. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static EvDocumentRecord CreateDocumentFromSearchResult(ScoredPoint result)
    {   // create document from search result
        var payload = result.Payload;
        
        return new EvDocumentRecord
        {
            Id           = result.Id.Uuid,
            FileName     = payload["fileName"].StringValue,
            Content      = payload["content"].StringValue,
            Vector       = ReadOnlyMemory<Single>.Empty, // not needed for search results
            ProcessedAt  = DateTime.ParseExact(
                payload["processedAt"].StringValue, 
                "O", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.RoundtripKind),
            FileHash     = payload["fileHash"].StringValue,
            ChunkIndex   = (Int32)payload["chunkIndex"].IntegerValue,
            TotalChunks  = (Int32)payload["totalChunks"].IntegerValue,
            Score        = result.Score // Include similarity score for search results
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Dispose resources. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public void Dispose()
    {   // dispose managed resources
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Protected dispose pattern implementation. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="disposing">   True to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
    ///-------------------------------------------------------------------------------------------------
    protected virtual void Dispose(Boolean disposing)
    {   // dispose pattern implementation
        if (!mDisposed && disposing)
        {   // dispose managed resources
            mQdrantClient?.Dispose();
            mDisposed = true;
        }

    }
}