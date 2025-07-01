using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Evanto.Mcp.Tools.SupportDocs.Config;
using Evanto.Mcp.Tools.SupportDocs.Contracts;
using Evanto.Mcp.Tools.SupportDocs.Services;
using Evanto.Mcp.Tools.SupportDocs.Repository;

namespace Evanto.Mcp.Tools.SupportDocs.Extensions;

/// <summary>
/// Extension-Methoden für ServiceCollection.
/// Created: 30.05.2025
/// </summary>
public static class EvSupportDocExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Registriert die ProductDocumentation Services. </summary>
    ///
    /// <remarks>   SvK, 30.05.2025. </remarks>
    ///
    /// <param name="services">     Die ServiceCollection. </param>
    /// <param name="configuration"> Die Konfiguration. </param>
    ///
    /// <returns>   Die ServiceCollection für Fluent-Interface. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddProductDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        // configure options
        services.Configure<EvSupportDocSettings>(
            configuration.GetSection("ProductDocumentation"));

        // register services
        services.AddScoped<IEvEmbeddingService, EvEmbeddingService>();
        services.AddScoped<IEvSupportDocsRepository, EvSupportDocsRepository>();

        return services;
    }
}
