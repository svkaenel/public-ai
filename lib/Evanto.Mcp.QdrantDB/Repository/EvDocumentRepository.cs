using System.Diagnostics;
using System.Globalization;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.QdrantDB.Contracts;
using Evanto.Mcp.QdrantDB.Models;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Evanto.Mcp.QdrantDB.Repository;

///-------------------------------------------------------------------------------------------------
/// <summary>   Unified document repository implementation using Qdrant. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class EvDocumentRepository(
    EvQdrantSettings                settings,
    IEvEmbeddingService             embeddingService,
    ILogger<EvDocumentRepository>   logger) : IEvDocumentRepository, IDisposable
{
    private readonly EvQdrantSettings                        mSettings           = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly IEvEmbeddingService                     mEmbeddingService   = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
    private readonly ILogger<EvDocumentRepository>           mLogger             = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly QdrantClient                            mQdrantClient       = new(settings?.QdrantEndpoint ?? throw new ArgumentNullException(nameof(settings)), settings?.QdrantPort ?? 6334);
    private          Boolean                                 mDisposed           = false;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initialize the repository and create collection if needed. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public async Task InitializeAsync()
    {
        try
        {   // check if collection exists and create if needed
            var collectionExists = await CollectionExistsAsync();
            
            if (!collectionExists)
            {
                mLogger.LogInformation("Creating collection: {CollectionName}", mSettings.VectorCollectionName);
                
                await mQdrantClient.CreateCollectionAsync(mSettings.VectorCollectionName, new VectorParams
                {
                    Size     = mSettings.VectorDimension,
                    Distance = Distance.Cosine
                });
                
                mLogger.LogInformation("Successfully created collection: {CollectionName}", mSettings.VectorCollectionName);
            }

            else
            {
                mLogger.LogDebug("Collection already exists: {CollectionName}", mSettings.VectorCollectionName);
            }
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to initialize vector store");
            throw new InvalidOperationException("Vector store initialization failed", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Check if the vector collection exists. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <returns>   True if collection exists, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> CollectionExistsAsync()
    {
        try
        {   // check collection existence
            var collections = await mQdrantClient.ListCollectionsAsync();

            return collections.Any(c => c == mSettings.VectorCollectionName);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogWarning(ex, "Failed to check collection existence");
            return false;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Check if a document exists in the repository. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="documentId">   Identifier for the document. </param>
    ///
    /// <returns>   True if document exists, false otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Boolean> DocumentExistsAsync(String documentId)
    {
        if (String.IsNullOrWhiteSpace(documentId))
        {
            mLogger.LogWarning("DocumentExistsAsync called with null or empty documentId");
            return false;
        }

        try
        {   // retrieve document by ID
            var pointIds = new List<PointId> { new PointId { Uuid = documentId } };
            var response = await mQdrantClient.RetrieveAsync(mSettings.VectorCollectionName, pointIds);
            
            return response.Any();
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogWarning(ex, "Failed to check document existence for: {DocumentId}", documentId);
            return false;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Store a single document in the repository. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="document">   The document to store. </param>
    ///-------------------------------------------------------------------------------------------------
    public async Task StoreDocumentAsync(EvDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        
        if (String.IsNullOrWhiteSpace(document.Id))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(document));

        try
        {   // create point and store document
            var point = CreatePointFromDocument(document);
            await mQdrantClient.UpsertAsync(mSettings.VectorCollectionName, new[] { point });
            
            mLogger.LogDebug("Stored document chunk: {DocumentId}", document.Id);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to store document: {DocumentId}", document.Id);
            throw new InvalidOperationException($"Failed to store document {document.Id}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Store multiple documents in the repository. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="documents">   The documents to store. </param>
    ///-------------------------------------------------------------------------------------------------
    public async Task StoreDocumentsAsync(IEnumerable<EvDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var documentList = documents.ToList();
        if (!documentList.Any())
        {
            mLogger.LogWarning("StoreDocumentsAsync called with empty document collection");
            return;
        }

        try
        {   // create points from documents and store batch
            var points = documentList.Select(CreatePointFromDocument).ToList();

            await mQdrantClient.UpsertAsync(mSettings.VectorCollectionName, points);
            
            mLogger.LogInformation("Stored {Count} document chunks", points.Count);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to store documents batch of {Count} items", documentList.Count);
            throw new InvalidOperationException("Failed to store document batch", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Delete a document from the repository. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="documentId">   Identifier for the document. </param>
    ///-------------------------------------------------------------------------------------------------
    public async Task DeleteDocumentAsync(String documentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(documentId);

        try
        {   // create point ID and delete document
            var pointId = new PointId { Uuid = documentId };

            await mQdrantClient.DeleteAsync(mSettings.VectorCollectionName, pointId.Num);   
            
            mLogger.LogDebug("Deleted document: {DocumentId}", documentId);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to delete document: {DocumentId}", documentId);
            throw new InvalidOperationException($"Failed to delete document {documentId}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Delete all documents associated with a file name. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="fileName">   Name of the file. </param>
    ///-------------------------------------------------------------------------------------------------
    public async Task DeleteDocumentsByFileNameAsync(String fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        try
        {   // get all documents for file and delete them
            var documents   = await GetDocumentsByFileNameAsync(fileName);
            var documentIds = documents.Select(d => new PointId { Uuid = d.Id }).Select(p => p.Num).ToList();
            
            if (documentIds.Any())
            {
                await mQdrantClient.DeleteAsync(mSettings.VectorCollectionName, documentIds);
                mLogger.LogInformation("Deleted {Count} documents for file: {FileName}", documentIds.Count, fileName);
            }

            else
            {
                mLogger.LogDebug("No documents found to delete for file: {FileName}", fileName);
            }
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to delete documents for file: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to delete documents for file {fileName}", ex);
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Search documents using flexible query parameters. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="query">   The search query parameters. </param>
    ///
    /// <returns>   Search results with matching documents. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvDocumentSearchResult> SearchAsync(EvDocumentSearchQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {   // determine search method and generate embedding if needed
            ReadOnlyMemory<Single> queryVector;
            String                 searchText;

            // Determine if we need to generate embeddings or use provided vector
            if (!query.QueryVector.IsEmpty)
            {
                queryVector = query.QueryVector;
                searchText  = "Vector Query";
            }
            
            else if (!String.IsNullOrWhiteSpace(query.QueryText))
            {
                mLogger.LogDebug("Generating embedding for query: {Query}", query.QueryText);

                queryVector = await mEmbeddingService.GenerateEmbeddingAsync(query.QueryText);
                searchText  = query.QueryText;
            }

            else
            {
                throw new ArgumentException("Either QueryText or QueryVector must be provided");
            }

            // Build search filter if needed
            Filter? filter = null;
            if (query.FileNameFilters.Any() || query.ProcessedAfter.HasValue || query.ProcessedBefore.HasValue)
            {
                filter = BuildSearchFilter(query);
            }

            // Perform search
            mLogger.LogDebug("Searching in collection {Collection} with limit {Limit}", 
                mSettings.VectorCollectionName, query.Limit);
            
            var searchResult = await mQdrantClient.SearchAsync(
                mSettings.VectorCollectionName,
                queryVector.ToArray(),
                filter: filter,
                limit: (UInt64)query.Limit,
                scoreThreshold: query.MinimumScore
            );

            stopwatch.Stop();

            if (searchResult == null || !searchResult.Any())
            {
                mLogger.LogInformation("No documents found for query: {Query}", searchText);
                return new EvDocumentSearchResult
                {
                    Documents       = Enumerable.Empty<EvDocument>(),
                    TotalCount      = 0,
                    Query           = searchText,
                    SearchDuration  = stopwatch.Elapsed,
                    Success         = true
                };
            }

            var documents = searchResult.Select(CreateDocumentFromSearchResult).ToList();
            
            mLogger.LogInformation("Found {Count} documents for query: {Query}", documents.Count, searchText);
            
            return new EvDocumentSearchResult
            {
                Documents       = documents,
                TotalCount      = documents.Count,
                Query           = searchText,
                SearchDuration  = stopwatch.Elapsed,
                Success         = true
            };
        }

        catch (Exception ex)
        {   // *log*
            stopwatch.Stop();
            mLogger.LogError(ex, "Failed to search documents");
            
            return new EvDocumentSearchResult
            {
                Documents       = Enumerable.Empty<EvDocument>(),
                TotalCount      = 0,
                Query           = query.QueryText,
                SearchDuration  = stopwatch.Elapsed,
                Success         = false,
                ErrorMessage    = ex.Message
            };
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Search documents by text query. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="queryText">   The text to search for. </param>
    /// <param name="limit">        Maximum number of results. </param>
    ///
    /// <returns>   Search results with matching documents. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvDocumentSearchResult> SearchByTextAsync(String queryText, Int32 limit = 10)
    {
        var query = new EvDocumentSearchQuery
        {
            QueryText       = queryText,
            Limit           = limit,
            MinimumScore    = mSettings.MinimumScore
        };
        
        return await SearchAsync(query);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Search documents by pre-computed vector. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="queryVector">   The query vector. </param>
    /// <param name="limit">         Maximum number of results. </param>
    ///
    /// <returns>   Search results with matching documents. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvDocumentSearchResult> SearchByVectorAsync(ReadOnlyMemory<Single> queryVector, Int32 limit = 10)
    {
        var query = new EvDocumentSearchQuery
        {
            QueryVector     = queryVector,
            Limit           = limit,
            MinimumScore    = mSettings.MinimumScore
        };
        
        return await SearchAsync(query);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Get unique file names from documents matching the query. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="queryText">   Optional text query to filter results. </param>
    /// <param name="limit">        Maximum number of results to consider. </param>
    ///
    /// <returns>   Collection of unique file names. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<String>> GetUniqueFileNamesAsync(String queryText = "", Int32 limit = 100)
    {
        try
        {   // search for documents and extract unique file names
            EvDocumentSearchResult searchResult;
            
            if (String.IsNullOrWhiteSpace(queryText))
            {
                // Get random documents to extract file names
                var randomQuery = new EvDocumentSearchQuery
                {
                    QueryText       = "*", // This might not work well - we'd need a different approach
                    Limit           = limit,
                    MinimumScore    = 0.0f
                };
                searchResult = await SearchAsync(randomQuery);
            }

            else
            {
                searchResult = await SearchByTextAsync(queryText, limit);
            }

            if (!searchResult.Success || !searchResult.Documents.Any())
            {
                return Enumerable.Empty<String>();
            }

            return searchResult.Documents
                .Select(d => d.FileName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to get unique file names");
            return Enumerable.Empty<String>();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Get all document chunks for a specific file. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="fileName">   Name of the file. </param>
    ///
    /// <returns>   All document chunks for the file. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<IEnumerable<EvDocument>> GetDocumentsByFileNameAsync(String fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        try
        {   // use scroll to get all documents for this file
            var filter = new Filter();
            filter.Must.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key     = "fileName",
                    Match   = new Match { Text = fileName }
                }
            });

            var searchResult = await mQdrantClient.SearchAsync(
                mSettings.VectorCollectionName,
                new Single[mSettings.VectorDimension], // Dummy vector
                filter: filter,
                limit: 1000 // Large limit to get all chunks
            );

            if (searchResult == null || !searchResult.Any())
            {
                return Enumerable.Empty<EvDocument>();
            }

            return searchResult
                .Select(CreateDocumentFromSearchResult)
                .Where(d => d.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(d => d.ChunkIndex)
                .ToList();
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to get documents for file: {FileName}", fileName);
            return Enumerable.Empty<EvDocument>();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Get total number of documents in the repository. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <returns>   Total document count. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<Int64> GetDocumentCountAsync()
    {
        try
        {   // get collection info and return point count
            var collectionInfo = await mQdrantClient.GetCollectionInfoAsync(mSettings.VectorCollectionName);

            return (Int64) (collectionInfo?.PointsCount ?? 0);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to get document count");
            return 0;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Get a specific document by ID. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="documentId">   Identifier for the document. </param>
    ///
    /// <returns>   The document if found, null otherwise. </returns>
    ///-------------------------------------------------------------------------------------------------
    public async Task<EvDocument?> GetDocumentByIdAsync(String documentId)
    {
        ArgumentException.ThrowIfNullOrEmpty(documentId);

        try
        {   // retrieve document by ID with payload and vectors
            var pointIds = new List<PointId> { new PointId { Uuid = documentId } };
            var response = await mQdrantClient.RetrieveAsync(mSettings.VectorCollectionName, pointIds, withPayload: true, withVectors: true);
            
            var point = response.FirstOrDefault();
            if (point == null)
            {
                return null;
            }

            return CreateDocumentFromRetrieveResult(point);
        }

        catch (Exception ex)
        {   // *log*
            mLogger.LogError(ex, "Failed to get document by ID: {DocumentId}", documentId);
            return null;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Create point from document for vector storage. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="document">   The document to convert. </param>
    ///
    /// <returns>   A PointStruct ready for storage. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static PointStruct CreatePointFromDocument(EvDocument document)
    {
        return new PointStruct
        {
            Id      = new PointId { Uuid = document.Id },
            Vectors = document.Vector.ToArray(),
            Payload =
            {
                ["fileName"]        = document.FileName,
                ["content"]         = document.Content,
                ["processedAt"]     = document.ProcessedAt.ToString("O", CultureInfo.InvariantCulture),
                ["fileHash"]        = document.FileHash,
                ["chunkIndex"]      = document.ChunkIndex,
                ["totalChunks"]     = document.TotalChunks,
                ["baseFileName"]    = document.BaseFileName,
                ["chunkId"]         = document.ChunkId
            }
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Create document from search result. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="result">   The search result to convert. </param>
    ///
    /// <returns>   An EvDocument from the search result. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static EvDocument CreateDocumentFromSearchResult(ScoredPoint result)
    {
        var payload = result.Payload;
        
        return new EvDocument
        {
            Id              = result.Id.Uuid,
            FileName        = payload["fileName"].StringValue,
            Content         = payload["content"].StringValue,
            Vector          = ReadOnlyMemory<Single>.Empty, // Not needed for search results
            ProcessedAt     = DateTime.ParseExact(
                                payload["processedAt"].StringValue, 
                                "O", 
                                CultureInfo.InvariantCulture, 
                                DateTimeStyles.RoundtripKind),
            FileHash        = payload["fileHash"].StringValue,
            ChunkIndex      = (Int32)payload["chunkIndex"].IntegerValue,
            TotalChunks     = (Int32)payload["totalChunks"].IntegerValue,
            BaseFileName    = payload.ContainsKey("baseFileName") ? payload["baseFileName"].StringValue : String.Empty,
            ChunkId         = payload.ContainsKey("chunkId") ? payload["chunkId"].StringValue : String.Empty,
            Score           = result.Score
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Create document from retrieve result. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="result">   The retrieve result to convert. </param>
    ///
    /// <returns>   An EvDocument from the retrieve result. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static EvDocument CreateDocumentFromRetrieveResult(RetrievedPoint result)
    {
        var payload = result.Payload;
        
        return new EvDocument
        {
            Id              = result.Id.Uuid,
            FileName        = payload["fileName"].StringValue,
            Content         = payload["content"].StringValue,
            Vector          = result.Vectors?.Vector?.Data != null ? new ReadOnlyMemory<Single>(result.Vectors.Vector.Data.ToArray()) : ReadOnlyMemory<Single>.Empty,
            ProcessedAt     = DateTime.ParseExact(
                                payload["processedAt"].StringValue, 
                                "O", 
                                CultureInfo.InvariantCulture, 
                                DateTimeStyles.RoundtripKind),
            FileHash        = payload["fileHash"].StringValue,
            ChunkIndex      = (Int32)payload["chunkIndex"].IntegerValue,
            TotalChunks     = (Int32)payload["totalChunks"].IntegerValue,
            BaseFileName    = payload.ContainsKey("baseFileName") ? payload["baseFileName"].StringValue : String.Empty,
            ChunkId         = payload.ContainsKey("chunkId") ? payload["chunkId"].StringValue : String.Empty,
            Score           = null // No score for direct retrieval
        };
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Build search filter from query parameters. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="query">   The query parameters to convert to filter. </param>
    ///
    /// <returns>   A Filter for the search operation. </returns>
    ///-------------------------------------------------------------------------------------------------
    private static Filter BuildSearchFilter(EvDocumentSearchQuery query)
    {
        var conditions = new List<Condition>();

        // File name filters
        if (query.FileNameFilters.Any())
        {
            var fileNameConditions = query.FileNameFilters.Select(fileName => new Condition
            {
                Field = new FieldCondition
                {
                    Key     = "fileName",
                    Match   = new Match { Text = fileName }
                }
            }).ToList();

            if (fileNameConditions.Count == 1)
            {
                conditions.Add(fileNameConditions[0]);
            }

            else
            {
                var orFilter = new Filter();
                foreach (var condition in fileNameConditions)
                {
                    orFilter.Should.Add(condition);
                }

                conditions.Add(new Condition
                {
                    Filter = orFilter
                });
            }
        }

        // Date range filters
        if (query.ProcessedAfter.HasValue)
        {
            conditions.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key     = "processedAt",
                    Range   = new Qdrant.Client.Grpc.Range
                    {
                        Gte = query.ProcessedAfter.Value.Ticks
                    }
                }
            });
        }

        if (query.ProcessedBefore.HasValue)
        {
            conditions.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key     = "processedAt",
                    Range   = new Qdrant.Client.Grpc.Range
                    {
                        Lt  = query.ProcessedBefore.Value.Ticks
                    }
                }
            });
        }

        var filter = new Filter();
        foreach (var condition in conditions)
        {
            filter.Must.Add(condition);
        }

        return filter;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Dispose resources. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Protected dispose pattern implementation. </summary>
    ///
    /// <remarks>   SvK, 04.07.2025. </remarks>
    ///
    /// <param name="disposing">   True if disposing managed resources. </param>
    ///-------------------------------------------------------------------------------------------------
    protected virtual void Dispose(Boolean disposing)
    {
        if (!mDisposed && disposing)
        {   // dispose managed resources
            mQdrantClient?.Dispose();
            mDisposed = true;
        }
    }
}