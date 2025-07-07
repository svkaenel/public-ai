using System.ComponentModel;
using Evanto.Mcp.Common.Mcp;
using Evanto.Mcp.QdrantDB.Contracts;
using ModelContextProtocol.Server;

namespace Evanto.Mcp.Tools.SupportDocs.Tools;

[McpServerToolType]
public class EvSupportDocsTool(IEvDocumentRepository documentRepository) : EvMcpToolBase
{
    private readonly IEvDocumentRepository mDocumentRepository = documentRepository;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>Gets the user with installations by name.</summary>
    /// 
    /// <param name="name"> The name. </param>
    /// 
    /// <returns> The user. </returns>
    ///-------------------------------------------------------------------------------------------------

    [McpServerTool, Description("Get support documentation related to query. Result is a list of file names that match the query.")]
    public async Task<String> GetDocumentNames(String query)
    {
        var validationError = ValidateNotEmpty(
            query,
            "Query must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mDocumentRepository.SearchByTextAsync(query, limit: 10),
            results => results == null || !results.Documents.Any(),
            results => results.Documents.Select(d => d.FileName).Distinct(StringComparer.OrdinalIgnoreCase),
            $"No support documents found for query '{query}'.");
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>Gets information from support documentation.</summary>
    ///
    /// <param name="query"> The Frage. </param>
    ///
    /// <returns> A JSON string representing the search results, or an error message if no documents 
    /// are found. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Get information from support documentation. Result is a list of documentation chunks that match the query.")]
    public async Task<String> GetInfosFromDocumentation(String query)
    {
        var validationError = ValidateNotEmpty(
            query,
            "Query must not be empty.");

        if (validationError != null)
            return validationError;

        // Repository-Call + NotFound + Error in einem Helper
        return await ExecuteAsync(
            () => mDocumentRepository.SearchByTextAsync(query, limit: 10),
            results => results == null || !results.Documents.Any(),
            results => results.Documents,
            $"No support documents found for query '{query}'.");
    }

}
