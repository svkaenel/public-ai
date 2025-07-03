using Evanto.Mcp.Vectorize.Models;

namespace Evanto.Mcp.Vectorize.Services;

public interface IEvVectorStoreService
{
    Task InitializeAsync();
    Task<bool> DocumentExistsAsync(string documentId);
    Task StoreDocumentAsync(EvDocumentRecord document);
    Task StoreDocumentsAsync(IEnumerable<EvDocumentRecord> documents);
    Task<IEnumerable<EvDocumentRecord>> SearchSimilarAsync(ReadOnlyMemory<float> queryVector, int limit = 10);
}
