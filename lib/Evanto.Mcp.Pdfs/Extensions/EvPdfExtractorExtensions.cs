using System;
using Evanto.Mcp.Pdfs.Contracts;
using Evanto.Mcp.Pdfs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Evanto.Mcp.Pdfs.Extensions;

public static class EvPdfExtractorExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds PDF text extraction service to the service collection. </summary>
    /// 
    /// <remarks>   SvK, 03.07.2025. </remarks>
    /// 
    /// <param name="services">   The service collection to extend. </param>
    /// 
    /// <returns>   The service collection with the added PDF text extraction service. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IServiceCollection AddPdfTextExtractor(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IEvPdfTextExtractorService, EvPdfTextExtractorService>();

        return services;
    }

}
