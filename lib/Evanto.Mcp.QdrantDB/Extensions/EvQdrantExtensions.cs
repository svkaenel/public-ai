using System;
using System.Linq;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.QdrantDB.Contracts;
using Evanto.Mcp.QdrantDB.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Evanto.Mcp.QdrantDB.Extensions;

///-------------------------------------------------------------------------------------------------
/// <summary>   Extension methods for Qdrant document repository registration. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public static class EvQdrantExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Add Qdrant document repository to the service collection. </summary>
    ///
    /// <param name="services">   The service collection. </param>
    /// <param name="settings">   The Qdrant settings. </param>
    ///
    /// <returns>   The service collection for chaining. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddQdrantDocumentRepository(
        this IServiceCollection services, 
        EvQdrantSettings        settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        services.AddSingleton(settings);
        services.AddScoped<IEvDocumentRepository, EvDocumentRepository>();

        return services;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Add Qdrant document repository to the service collection with configuration action. </summary>
    ///
    /// <param name="services">           The service collection. </param>
    /// <param name="configureSettings"> Action to configure the settings. </param>
    ///
    /// <returns>   The service collection for chaining. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddQdrantDocumentRepository(
        this IServiceCollection     services,
        Action<EvQdrantSettings>    configureSettings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureSettings);

        var settings = new EvQdrantSettings();
        configureSettings(settings);

        return services.AddQdrantDocumentRepository(settings);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Add Qdrant document repository with embedding service dependency. </summary>
    ///
    /// <param name="services">   The service collection. </param>
    /// <param name="settings">   The Qdrant settings. </param>
    ///
    /// <returns>   The service collection for chaining. </returns>
    ///
    /// <remarks>   
    /// This method ensures that IEvEmbeddingService is available in the service collection.
    /// Call this after registering your embedding service.
    /// </remarks>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddQdrantDocumentRepositoryWithEmbeddings(
        this IServiceCollection services, 
        EvQdrantSettings        settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        // Validate that embedding service is registered
        var embeddingServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEvEmbeddingService));
        if (embeddingServiceDescriptor == null)
        {
            throw new InvalidOperationException(
                "IEvEmbeddingService must be registered before calling AddQdrantDocumentRepositoryWithEmbeddings. " +
                "Register your embedding service first using the appropriate extension method.");
        }

        return services.AddQdrantDocumentRepository(settings);
    }
}