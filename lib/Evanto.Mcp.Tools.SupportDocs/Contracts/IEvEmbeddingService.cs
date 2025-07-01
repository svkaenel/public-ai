using System;
using System.Threading.Tasks;

namespace Evanto.Mcp.Tools.SupportDocs.Contracts;

/// <summary>
/// Interface für Embedding Service.
/// Created: 30.05.2025
/// </summary>
public interface IEvEmbeddingService
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Generiert ein Embedding für den gegebenen Text. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="text">    Der zu verarbeitende Text. </param>
    ///
    /// <returns>   Das generierte Embedding als ReadOnlyMemory. </returns>
    ///-------------------------------------------------------------------------------------------------
    Task<ReadOnlyMemory<Single>> GenerateEmbeddingAsync(String text);
}
