using System;
using System.Threading.Tasks;

namespace Evanto.Mcp.Tools.SupportDocs.Contracts;

public interface IEvEmbeddingService
{
    Task<ReadOnlyMemory<Single>> GenerateEmbeddingAsync(String text);
}
