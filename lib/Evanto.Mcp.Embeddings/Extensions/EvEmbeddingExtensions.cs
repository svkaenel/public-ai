using System;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Embeddings.Contracts;
using Evanto.Mcp.Embeddings.Factories;
using Evanto.Mcp.Embeddings.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Evanto.Mcp.Embeddings.Extensions;

public static class EvEmbeddingExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds embedding service to the service collection. </summary>
    /// 
    /// <remarks>   SvK, 03.07.2025. </remarks>
    /// 
    /// <param name="services">   The service collection to extend. </param>
    /// <param name="loggerFactory">   The logger factory to use for logging. </param>
    /// <param name="appSettings">  The application settings containing provider configuration. </param
    /// 
    /// <returns>   The service collection with the added embedding service. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddEmbeddings(
        this IServiceCollection services,
        ILoggerFactory          loggerFactory,
        EvBaseAppSettings       appSettings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggerFactory, "Logger factory must be valid!");
        ArgumentNullException.ThrowIfNull(appSettings, "Application settings must be valid!");

        services.AddScoped<IEvEmbeddingService, EvEmbeddingService>();

        var provider = appSettings.GetDefaultEmbeddingProvider();

        ArgumentNullException.ThrowIfNull(provider, "Default provider must be configured in application settings!");

        var embeddingGeneratorFactory = EvEmbeddingGeneratorFactory.Create(loggerFactory);
        var embeddingGenerator        = embeddingGeneratorFactory.CreateEmbeddingGenerator(
            provider, 
            provider.DefaultModel);

        services.AddEmbeddingGenerator(embeddingGenerator);
        services.AddSingleton(provider);

        return services;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Builds an embedding generator with distributed cache and OpenTelemetry support. </summary>
    /// /// 
    /// <remarks>   SvK, 07.07.2025. </remarks>
    /// 
    /// <param name="generator"> The embedding generator to enhance. </param>
    /// <param name="settings">  The embedding settings to use. </param>
    ///  
    /// <returns>   An IEmbeddingGenerator with distributed cache and OpenTelemetry support. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IEmbeddingGenerator<String, Embedding<Single>> Build(this IEmbeddingGenerator<string, Embedding<Single>> generator, EvChatClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(generator, "Embedding generator must be valid!");
        ArgumentNullException.ThrowIfNull(settings, "Embedding settings must be valid!");

        // Create and return the embedding generator with distributed cache and OpenTelemetry
        return new EmbeddingGeneratorBuilder<string, Embedding<Single>>(generator)
            .UseDistributedCache(
            new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()))
            )
        .UseOpenTelemetry(sourceName: $"{settings.ProviderName}-embeddings")
        .Build();
    }
}
