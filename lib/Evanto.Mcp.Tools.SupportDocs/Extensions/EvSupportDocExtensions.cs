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

    /*
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the product documentation database access. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="app">  The application to extend. </param>
    ///
    /// <returns>   True if it succeeds, false if it fails. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<Boolean> TestPrDocAccessAsync(this IHost app)
    {   // check requirements
        app.Should().NotBeNull("Host application must be valid!");

        try
        {   // get the repository
            var repository = app.Services.CreateScope().ServiceProvider.GetService<IProductDocumentationRepository>();
            if (repository == null)
            {
                return false;
            }

            // AA_19953_Aufbauanleitung_HKD_5.1_de.21.pdf
            var documentation = await repository.GetProductDocumentationAsync("HKD 5.1");
            // var viewModel    = documentation != null ? new ProductRegistrationViewModel().InitFrom(documentation) : null;
            var ok = documentation != null && documentation.Any();

            return ok;
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error testing Product Documentation DB access: {ex.Message}");
        }

        return false;
    }*/
}
