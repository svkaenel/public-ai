using System;
using Evanto.Mcp.QdrantDB.Models;

namespace Evanto.Mcp.QdrantDB.Contracts;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for unified document repository operations. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface IEvDocumentRepository : IDisposable
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Initialization operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    
    /// <summary>   Initialize the repository and create collection if needed. </summary>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    Task InitializeAsync();

    /// <summary>   Check if the vector collection exists. </summary>
    /// <returns>   True if collection exists, false otherwise. </returns>
    Task<Boolean> CollectionExistsAsync();

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Storage operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    
    /// <summary>   Check if a document exists in the repository. </summary>
    /// <param name="documentId">   Identifier for the document. </param>
    /// <returns>   True if document exists, false otherwise. </returns>
    Task<Boolean> DocumentExistsAsync(String documentId);

    /// <summary>   Store a single document in the repository. </summary>
    /// <param name="document">   The document to store. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    Task StoreDocumentAsync(EvDocument document);

    /// <summary>   Store multiple documents in the repository. </summary>
    /// <param name="documents">   The documents to store. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    Task StoreDocumentsAsync(IEnumerable<EvDocument> documents);

    /// <summary>   Delete a document from the repository. </summary>
    /// <param name="documentId">   Identifier for the document. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    Task DeleteDocumentAsync(String documentId);

    /// <summary>   Delete all documents associated with a file name. </summary>
    /// <param name="fileName">   Name of the file. </param>
    /// <returns>   A Task representing the asynchronous operation. </returns>
    Task DeleteDocumentsByFileNameAsync(String fileName);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Search operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    
    /// <summary>   Search documents using flexible query parameters. </summary>
    /// <param name="query">   The search query parameters. </param>
    /// <returns>   Search results with matching documents. </returns>
    Task<EvDocumentSearchResult> SearchAsync(EvDocumentSearchQuery query);

    /// <summary>   Search documents by text query. </summary>
    /// <param name="queryText">   The text to search for. </param>
    /// <param name="limit">        Maximum number of results. </param>
    /// <returns>   Search results with matching documents. </returns>
    Task<EvDocumentSearchResult> SearchByTextAsync(String queryText, Int32 limit = 10);

    /// <summary>   Search documents by pre-computed vector. </summary>
    /// <param name="queryVector">   The query vector. </param>
    /// <param name="limit">         Maximum number of results. </param>
    /// <returns>   Search results with matching documents. </returns>
    Task<EvDocumentSearchResult> SearchByVectorAsync(ReadOnlyMemory<Single> queryVector, Int32 limit = 10);

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Utility operations. </summary>
    ///-------------------------------------------------------------------------------------------------
    
    /// <summary>   Get unique file names from documents matching the query. </summary>
    /// <param name="queryText">   Optional text query to filter results. </param>
    /// <param name="limit">        Maximum number of results to consider. </param>
    /// <returns>   Collection of unique file names. </returns>
    Task<IEnumerable<String>> GetUniqueFileNamesAsync(String queryText = "", Int32 limit = 100);

    /// <summary>   Get all document chunks for a specific file. </summary>
    /// <param name="fileName">   Name of the file. </param>
    /// <returns>   All document chunks for the file. </returns>
    Task<IEnumerable<EvDocument>> GetDocumentsByFileNameAsync(String fileName);

    /// <summary>   Get total number of documents in the repository. </summary>
    /// <returns>   Total document count. </returns>
    Task<Int64> GetDocumentCountAsync();

    /// <summary>   Get a specific document by ID. </summary>
    /// <param name="documentId">   Identifier for the document. </param>
    /// <returns>   The document if found, null otherwise. </returns>
    Task<EvDocument?> GetDocumentByIdAsync(String documentId);
}