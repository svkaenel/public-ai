using System;
using System.ComponentModel;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using ModelContextProtocol.Server;

namespace Evanto.Mcp.Tools.SupportDocs.Tools;

[McpServerToolType]
public class EvSupportDocsTool(IEvSupportDocsRepository ProductDocumentationRepository)
{
    private readonly IEvSupportDocsRepository mProductDocumentationRepository = ProductDocumentationRepository;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>Gets the user with installations by name.</summary>
    /// 
    /// <param name="name"> The name. </param>
    /// 
    /// <returns> The user. </returns>
    ///-------------------------------------------------------------------------------------------------

    [McpServerTool, Description("Finde Anleitungen und Dokumentationen zu BRUNNER Produkten. Ergebnis ist eine Liste von Dateinamen, die in der Datenbank gespeichert sind.")]
    public async Task<String> FindeAnleitungen(String produktName)
    {
        try
        {   // validate input
            if (String.IsNullOrWhiteSpace(produktName))
            {
                return new { status = "error", message = "Query not be empty.." }.ToJson();
            }

            var results = await mProductDocumentationRepository.GetFileNames(produktName);

            if (results.Count() == 0)
            {
                return new { status = "not_found", message = $"No result documentation found for query {produktName}" }.ToJson();
            }

            return new { status = "success", data = results }.ToJson();
        }

        catch (Exception ex)
        {   // log error to stderr  
            Console.Error.WriteLine($"Error in FindeAnleitungen() for '{produktName}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>Finde Informationen zu BRUNNER Produkten in Anleitungen und Dokumentationen.</summary>
    ///
    /// <param name="frage"> The Frage. </param>
    ///
    /// <returns> Informationen zu BRUNNER Produkten in Anleitungen und Dokumentationen. </returns>
    ///-------------------------------------------------------------------------------------------------
    [McpServerTool, Description("Finde Informationen zu zu BRUNNER Produkten in Anleitungen und Dokumentationen. Der Produktname oder der Dateiname sollte immer in der Frage enthalten sein.")]
    public async Task<String> FindeInfosInAnleitungen(String frage)
    {
        try
        {   // validate input
            if (String.IsNullOrWhiteSpace(frage))
            {
                return new { status = "error", message = "Query not be empty.." }.ToJson();
            }

            var results = await mProductDocumentationRepository.GetProductDocumentationAsync(frage);

            if (results.Count() == 0)
            {
                return new { status = "not_found", message = $"No result documentation found for query {frage}" }.ToJson();
            }

            // var viewModel = new CompanyViewModel().InitFrom(results);

            return new { status = "success", data = results }.ToJson();
        }

        catch (Exception ex)
        {   // log error to stderr  
            Console.Error.WriteLine($"Error in FindeAnleitungen() for '{frage}': {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new { status = "error", message = ex.Message, details = ex.GetType().Name }.ToJson();
        }
    }

}
