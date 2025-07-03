using System;
using System.Threading.Tasks;

namespace Evanto.Mcp.Embeddings.Contracts;

public interface IEvEmbeddingService
{
    Task<ReadOnlyMemory<Single>>        GenerateEmbeddingAsync(String text);
    Task<List<ReadOnlyMemory<Single>>> GenerateEmbeddingsAsync(IEnumerable<String> texts);
}
