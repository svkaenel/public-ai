using Microsoft.Extensions.DependencyInjection;
using Evanto.Mcp.Vectorize.Services;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Embeddings.Services;
using Evanto.Mcp.Embeddings.Contracts;

namespace Evanto.Mcp.Vectorize.Extensions;

public static class EvVectorizeAppExtensions
{
    public static IServiceCollection AddPdfVectorizationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IEvPdfTextExtractor, EvPdfTextExtractor>();
        services.AddScoped<IEvEmbeddingService, EvEmbeddingService>();
        services.AddScoped<IEvVectorStoreService, EvVectorStoreService>();
        services.AddScoped<IEvFileTrackingService, EvFileTrackingService>();
        services.AddScoped<IEvPdfProcessingService, EvPdfProcessingService>();

        return services;
    }
}
