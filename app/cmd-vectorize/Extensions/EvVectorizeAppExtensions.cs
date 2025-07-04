using Microsoft.Extensions.DependencyInjection;
using Evanto.Mcp.Vectorize.Services;
using Evanto.Mcp.Vectorize.Contracts;
using Evanto.Mcp.Embeddings.Services;
using Evanto.Mcp.Embeddings.Contracts;

namespace Evanto.Mcp.Vectorize.Extensions;

///-------------------------------------------------------------------------------------------------
/// <summary>   Extension methods for vectorization services registration. </summary>
///
/// <remarks>   SvK, 03.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public static class EvVectorizeAppExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds PDF vectorization services to the service collection. </summary>
    ///
    /// <remarks>   SvK, 03.07.2025. </remarks>
    ///
    /// <param name="services">   The service collection. </param>
    ///
    /// <returns>   The service collection for chaining. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddPdfVectorizationServices(this IServiceCollection services)
    {   // register all vectorization services
        services.AddScoped<IEvFileTrackingService, EvFileTrackingService>();
        services.AddScoped<IEvPdfProcessingService, EvPdfProcessingService>();

        return services;
    }
}
