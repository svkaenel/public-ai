using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using Evanto.Mcp.Tools.SupportDocs.Services;
using Evanto.Mcp.Tools.SupportDocs.Repository;
using Microsoft.Extensions.Hosting;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportDocs.Tools;

namespace Evanto.Mcp.Tools.SupportDocs.Extensions;


public static class EvSupportDocExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds support documentation services to the specified service collection. </summary>
    /// <remarks>   SvK, 03.06.2025. </remarks>
    /// 
    /// <param name="services">   The service collection to extend. </param>
    ///
    /// <returns>   The service collection with the added support documentation services. </returns>
    ///------------------------------------------------------------------------------------------------
    public static IServiceCollection AddSupportDocs(this IServiceCollection services, EvMcpSrvAppSettings settings)
    {   // check requirements
        ArgumentNullException.ThrowIfNull(services, "Service collection must be valid!");
        ArgumentNullException.ThrowIfNull(settings, "Settings must be valid!");
        ArgumentNullException.ThrowIfNull(settings.QdrantSettings, "Qdrant settings must be valid!");
        ArgumentNullException.ThrowIfNull(settings.EmbeddingSettings, "Embedding settings must be valid!");

        // register services
        services.AddScoped<IEvEmbeddingService, EvEmbeddingService>();
        services.AddScoped<IEvSupportDocsRepository, EvSupportDocsRepository>();

        services.AddSingleton(settings.EmbeddingSettings);
        services.AddSingleton(settings.QdrantSettings);

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
            var repository = app.Services.CreateScope().ServiceProvider.GetService<IEvSupportDocsRepository>();
            if (repository == null)
            {
                return false;
            }

            // test the support documentation access
            var documentation = await repository.GetSupportDocsAsync(query);
            // var viewModel    = documentation != null ? new ProductRegistrationViewModel().InitFrom(documentation) : null;
            var ok = documentation != null && documentation.Any();

            return ok;
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error testing Support Documentation DB access: {ex.Message}");
        }

        return false;
    }
}
