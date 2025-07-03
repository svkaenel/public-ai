using System;
using System.ComponentModel;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using ModelContextProtocol.Server;

namespace Evanto.Mcp.Tools.SupportDocs.Tools;

[McpServerToolType]
public class EvSupportDocsTool(IEvSupportDocsRepository supportDocsRepository)
{
    private readonly IEvSupportDocsRepository mSupportDocsRepository = supportDocsRepository;

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
        try
        {   // validate input
            if (String.IsNullOrWhiteSpace(query))
            {
                return new { status = "error", message = "Query not be empty.." }.ToJson();
            }

            var results = await mSupportDocsRepository.GetFileNames(query);

            if (results.Count() == 0)
            {
                return new { status = "not_found", message = $"No result documentation found for query {query}" }.ToJson();
            }

            return new { status = "success", data = results }.ToJson();
        }

        catch (Exception ex)
        {   // log error to stderr  
            Console.Error.WriteLine($"Error in GetDocumentNames() for '{query}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>Finde Informationen zu BRUNNER Produkten in Anleitungen und Dokumentationen.</summary>
    ///
    /// <param name="query"> The Frage. </param>
    ///
    /// <returns> Informationen zu BRUNNER Produkten in Anleitungen und Dokumentationen. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Get information from support documentation. Result is a list of documentation chunks that match the query.")]
    public async Task<String> GetInfosFromDocumentation(String query)
    {
        try
        {   // validate input
            if (String.IsNullOrWhiteSpace(query))
            {
                return new { status = "error", message = "Query not be empty.." }.ToJson();
            }

            var results = await mSupportDocsRepository.GetSupportDocsAsync(query);

            if (results.Count() == 0)
            {
                return new { status = "not_found", message = $"No result documentation found for query {query}" }.ToJson();
            }

            return new { status = "success", data = results }.ToJson();
        }

        catch (Exception ex)
        {   // log error to stderr  
            Console.Error.WriteLine($"Error in GetInfosFromDocumentation() for '{query}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

}
