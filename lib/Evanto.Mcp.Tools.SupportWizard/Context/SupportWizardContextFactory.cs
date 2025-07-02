using System;
using Evanto.Mcp.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Evanto.Mcp.Tools.SupportWizard.Context;

public class SupportWizardContextFactory : IDesignTimeDbContextFactory<SupportWizardDbContext>
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>  Creates database context. </summary>
    ///
    /// <remarks>  SvK, 02.07.2025. </remarks>
    ///
    /// <param name="args">    The arguments. </param>
    ///
    /// <returns>  The new database context. </returns>
    ///-------------------------------------------------------------------------------------------------
    
    public SupportWizardDbContext CreateDbContext(String[] args)
    {   // app settings file is in api directory
        var optionsBuilder  = new DbContextOptionsBuilder<SupportWizardDbContext>();

        var environment     = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (String.IsNullOrWhiteSpace(environment))
        {   // if not in ASP.NET context
            environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.factory.json")
            .Build();

        var connectionString = configuration.GetConnectionString(ProjectConstants.DEF_CONNECTION_STRING);
        
        optionsBuilder.UseSqlite(connectionString, o => o.MigrationsAssembly("Evanto.Mcp.Tools.SupportWizard"));
        
        return new SupportWizardDbContext(optionsBuilder.Options);
    }
}
