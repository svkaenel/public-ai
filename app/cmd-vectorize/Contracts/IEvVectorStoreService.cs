using Evanto.Mcp.Vectorize.Models;

namespace Evanto.Mcp.Vectorize.Services;

///-------------------------------------------------------------------------------------------------
/// <summary>   Interface for vector store service. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public interface IEvVectorStoreService
{
    Task                                        InitializeAsync();
    Task<Boolean>                               DocumentExistsAsync(String documentId);
    Task                                        StoreDocumentAsync(EvDocumentRecord document);
    Task                                        StoreDocumentsAsync(IEnumerable<EvDocumentRecord> documents);
    Task<IEnumerable<EvDocumentRecord>>         SearchSimilarAsync(ReadOnlyMemory<Single> queryVector, Int32 limit = 10);
}
