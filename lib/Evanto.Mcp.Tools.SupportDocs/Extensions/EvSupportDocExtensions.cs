using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportDocs.Tools;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.Embeddings.Services;
using Evanto.Mcp.QdrantDB.Contracts;
using Evanto.Mcp.QdrantDB.Repository;
using Evanto.Mcp.QdrantDB.Extensions;
using Evanto.Mcp.Embeddings.Extensions;
using Microsoft.Extensions.Logging;

namespace Evanto.Mcp.Tools.SupportDocs.Extensions;


public static class EvSupportDocExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds support documentation services to the specified service collection. </summary>
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// 
    /// <param name="services">         The service collection to extend. </param>
    /// <param name="loggerFactory">    The logger factory to use for logging. </param>
    /// <param name="settings">         The application settings containing Qdrant and embedding configurations
    ///
    /// <returns>   The service collection with the added support documentation services. </returns>
    ///------------------------------------------------------------------------------------------------
    public static IServiceCollection AddSupportDocs(this IServiceCollection services, ILoggerFactory loggerFactory, EvMcpSrvAppSettings settings)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(services, "Service collection must be valid!");
        ArgumentNullException.ThrowIfNull(settings, "Settings must be valid!");
        ArgumentNullException.ThrowIfNull(settings.Qdrant, "Qdrant settings must be valid!");

        // register services
        services.AddQdrantDocumentRepository(settings.Qdrant);
        services.AddEmbeddings(loggerFactory, settings);

        return services;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds the support docs MCP tools to the server builder. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="builder"> The builder to extend. </param>
    /// 
    /// <returns>   An IMcpServerBuilder. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IMcpServerBuilder WithSupportDocMcpTools(this IMcpServerBuilder builder)
    {   // settings are need for the client
        builder.WithTools<EvSupportDocsTool>();
        // return the service collection
        return builder;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the support documentation database access. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="app">  The application to extend. </param>
    ///
    /// <returns>   True if it succeeds, false if it fails. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<Boolean> TestSupportDocsAccessAsync(this IHost app, String query)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(app, "Host application must be valid!");
        ArgumentNullException.ThrowIfNullOrEmpty(query, "Query must be valid!");

        try
        {   // get the repository
            var repository = app.Services.CreateScope().ServiceProvider.GetService<IEvDocumentRepository>();
            if (repository == null)
            {
                return false;
            }

            // test the support documentation access
            var searchResult = await repository.SearchByTextAsync(query, limit: 5);
            var ok = searchResult != null && searchResult.Success && searchResult.Documents.Any();

            return ok;
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error testing Support Documentation DB access: {ex.Message}");
        }

        return false;
    }
}
