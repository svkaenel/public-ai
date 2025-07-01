using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Evanto.Mcp.Tools.SupportWizard.Context;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportWizard.Contracts;

namespace Brunner.Mcp.Server;

/// <summary>
/// Extension-Methoden für die Konfiguration von Datenbanken und Tests in der br-mcp-server Anwendung.
/// Created: 03.06.2025
/// </summary>
public static class BrMcpServerExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds the support wizard database. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="builder">      The builder to extend. </param>
    /// <param name="settings">     The settings. </param>
    ///
    /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are null. </exception>
    ///-------------------------------------------------------------------------------------------------
    public static IHostApplicationBuilder AddSupportWizardDB(this IHostApplicationBuilder builder, EvMcpSrvAppSettings? settings = null)
    {   // check requirements
        builder.Should().NotBeNull("Host application builder must be valid!");
        
        var connectWizard = builder.Configuration.GetConnectionString("SupportWizardDB");

        connectWizard.Should().NotBeNullOrEmpty("BRUNNER product registration connection string must be valid!");

        // add mybrunner + zbv database with minimal logging
        builder.Services.AddDbContext<SupportWizardContext>(options =>
        {
            options.UseSqlite(connectWizard);
            // Disable sensitive data logging and detailed errors for production
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            // Suppress EF Core logging completely to avoid stdout interference
            options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter(_ => false)));
        });

        return builder;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the brunner product registration database access. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="app">  The application to extend. </param>
    ///
    /// <returns>   True if it succeeds, false if it fails. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<Boolean> TestSupportWizardAccessAsync(this IHost app)
    {   // check requirements
        app.Should().NotBeNull("Host application must be valid!");

        try
        {   // get the repository
            var repository = app.Services.CreateScope().ServiceProvider.GetService<ISupportWizardRepository>();
            if (repository == null)
            {
                return false;
            }

            var users        = await repository.GetAllUsersAsync();
            var ok           = users.Count() >= 0;

            return ok;
        }

        catch (Exception ex)
        {   
            Console.Error.WriteLine($"Error testing Support Wizard DB access: {ex.Message}");
        }

        return false;
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
