using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Evanto.Mcp.Tools.SupportDocs.ViewModels;

namespace Evanto.Mcp.Tools.SupportDocs.Contracts;

/// <summary>
/// Interface für Produktdokumentation Repository.
/// Created: 30.05.2025
/// </summary>
public interface IEvSupportDocsRepository
{
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
    Task<IEnumerable<EvSupportDocViewModel>> GetProductDocumentationAsync(String query, Int32 limit = 10);
    Task<IEnumerable<String>> GetFileNames(String query, Int32 limit = 10);
}
