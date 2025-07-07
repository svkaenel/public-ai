using System;
using Evanto.Mcp.Common.Settings;
using Evanto.Mcp.Tools.SupportWizard.Context;
using Evanto.Mcp.Tools.SupportWizard.Contracts;
using Evanto.Mcp.Tools.SupportWizard.Repository;
using Evanto.Mcp.Tools.SupportWizard.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLitePCL;

namespace Evanto.Mcp.Tools.SupportWizard.Extensions;

public static class EvSupportWizardExtensions
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds support wizard services to the specified host application builder. </summary>
    ///  
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="builder"> The host application builder to extend. </param>
    /// 
    /// <returns>   An IHostApplicationBuilder with the added support wizard services. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IHostApplicationBuilder AddSupportWizard(this IHostApplicationBuilder builder)
    {   // check requirements
        ArgumentNullException.ThrowIfNull("Host application builder must be valid!");

        builder.AddSupportWizardDB();

        builder.Services
            .AddSupportWizardServices();

        return builder;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds the support wizard MCP tools to the server builder. </summary>
    /// 
    /// <remarks>   SvK, 01.07.2025. </remarks>
    /// 
    /// <param name="builder"> The builder to extend. </param>
    /// 
    /// <returns>   An IMcpServerBuilder. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static IMcpServerBuilder WithSupportWizardMcpTools(this IMcpServerBuilder builder)
    {   // settings are need for the client
        builder.WithTools<EvSupportWizardTool>();
        // return the service collection
        return builder;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Tests the support wizard database access. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="app">  The application to extend. </param>
    ///
    /// <returns>   True if it succeeds, false if it fails. </returns>
    ///-------------------------------------------------------------------------------------------------
    public static async Task<Boolean> TestSupportWizardAccessAsync(this IHost app)
    {   // check requirements
        ArgumentNullException.ThrowIfNull("Host application must be valid!");

        try
        {   // get the repository
            var repository = app.Services.CreateScope().ServiceProvider.GetService<ISupportWizardRepository>();
            if (repository == null)
            {
                return false;
            }

            var users = await repository.GetAllUsersAsync();
            var ok = users.Count() >= 0;

            return ok;
        }

        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error testing Support Wizard DB access: {ex.Message}");
        }

        return false;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>	Migrate database. </summary>
    ///
    /// <remarks>	SvK, 20.02.2024. </remarks>
    ///
    /// <param name="app">	The application. </param>
    ///
    /// <returns>	True if it succeeds, false if it fails. </returns>
    ///-------------------------------------------------------------------------------------------------

    public static Boolean MigrateDatabase(this IHost app)
    {
        try
        {   // check if database connection is available
            var context = app.Services.CreateScope().ServiceProvider.GetService<SupportWizardDbContext>();
            if (context == null)
            {
                return false;
            }

            context.Database.Migrate();

            return true;
        }

        catch (Exception ex)
        {
            Console.WriteLine("Migrating database failed: {0}!", ex.Message);
            return false;
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Add support wizard MCP tools. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="services"> The services to act on. </param>
    ///
    /// <returns>   An IServiceCollection. </returns>
    ///-------------------------------------------------------------------------------------------------

    private static IServiceCollection AddSupportWizardServices(this IServiceCollection services)
    {   // settings are need for the client
        services.AddScoped<ISupportWizardRepository, SupportWizardRepository>();
        // return the service collection
        return services;
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Adds the support wizard database. </summary>
    ///
    /// <remarks>   SvK, 03.06.2025. </remarks>
    ///
    /// <param name="builder">      The builder to extend. </param>
    ///
    /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are null. </exception>
    ///-------------------------------------------------------------------------------------------------
    private static IHostApplicationBuilder AddSupportWizardDB(this IHostApplicationBuilder builder)
    {   // check requirements
        ArgumentNullException.ThrowIfNull("Host application builder must be valid!");

        var connectDB = builder.Configuration.GetConnectionString("SupportWizardDB");

        ArgumentException.ThrowIfNullOrEmpty("Support wizard connection string must be valid!");

        // Initialize SQLite with system provider for Alpine Linux containers
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());

        // add support wizard database with minimal logging
        builder.Services.AddDbContext<SupportWizardDbContext>(options =>
        {
            options.UseSqlite(connectDB);
            // Disable sensitive data logging and detailed errors for production
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            // Suppress EF Core logging completely to avoid stdout interference
            options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter(_ => false)));
        });

        return builder;
    }

}
