using System;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.Embeddings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Evanto.Mcp.Embeddings.Extensions;

public static class EvEmbeddingExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds embedding service to the service collection. </summary>
    /// 
    /// <remarks>   SvK, 03.07.2025. </remarks>
    /// 
    /// <param name="services">   The service collection to extend. </param>
    /// 
    /// <returns>   The service collection with the added embedding service. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddEmbeddings(
        this IServiceCollection services,
        EvEmbeddingSettings?    settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings, "Embedding settings must be valid!");

        services.AddScoped<IEvEmbeddingService, EvEmbeddingService>();
        services.AddSingleton(settings);

        return services;
    }

}
