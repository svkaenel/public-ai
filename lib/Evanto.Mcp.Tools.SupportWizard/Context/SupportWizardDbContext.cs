using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Evanto.Mcp.Tools.SupportWizard.Models;

namespace Evanto.Mcp.Tools.SupportWizard.Context;

///-------------------------------------------------------------------------------------------------
/// <summary>   Entity Framework DbContext for SupportWizard database. </summary>
///
/// <remarks>   SvK, 01.07.2025. </remarks>
///-------------------------------------------------------------------------------------------------
public class SupportWizardDbContext : DbContext
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Default constructor. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    public SupportWizardDbContext()
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Constructor with options. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="options"> The options. </param>
    ///-------------------------------------------------------------------------------------------------
    public SupportWizardDbContext(DbContextOptions<SupportWizardDbContext> options) : base(options)
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the support requests. </summary>
    ///
    /// <value> The support requests. </value>
    ///-------------------------------------------------------------------------------------------------
    public DbSet<SupportRequest> SupportRequests { get; set; } = null!;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Gets or sets the users. </summary>
    ///
    /// <value> The users. </value>
    ///-------------------------------------------------------------------------------------------------
    public DbSet<User> Users { get; set; } = null!;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configure the database connection. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="optionsBuilder"> The options builder. </param>
    ///-------------------------------------------------------------------------------------------------
    /* optionally
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (String.IsNullOrWhiteSpace(environment))
            {   // if not in ASP.NET context
                environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            // Default connection string for migrations
            var connectionString = configuration?.GetConnectionString("SupportWizardDB")
                ?? "Data Source=SupportWizard.db";

            optionsBuilder.UseSqlite(connectionString, options =>
            {
                options.CommandTimeout(30);
            });

            // Enable detailed logging in development
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
#endif
        }
    }
    */
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Configure the model. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="modelBuilder"> The model builder. </param>
    ///-------------------------------------------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SupportRequest entity
        modelBuilder.Entity<SupportRequest>(entity =>
        {
            entity.ToTable("SupportRequests");
            
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Uid).ValueGeneratedOnAdd();
            
            entity.Property(e => e.CustomerEmail)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.ReceivedAt)
                .IsRequired();
            
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("TEXT");
            
            entity.Property(e => e.Topic)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<byte>();
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.AssignedToUserUid)
                .IsRequired(false);
            
            entity.Property(e => e.ResolvedAt)
                .IsRequired(false);
            
            entity.Property(e => e.ResolutionNotes)
                .IsRequired(false)
                .HasColumnType("TEXT");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Configure foreign key relationship
            entity.HasOne(e => e.AssignedToUser)
                .WithMany(u => u.AssignedSupportRequests)
                .HasForeignKey(e => e.AssignedToUserUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for performance
            entity.HasIndex(e => e.CustomerEmail);
            entity.HasIndex(e => e.CustomerName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Topic);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.AssignedToUserUid);
            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Uid).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Topic)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // Configure indexes
            entity.HasIndex(e => e.Email)
                .IsUnique();
            entity.HasIndex(e => e.Topic);
        });
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Override SaveChanges to automatically update timestamps. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <returns>   The number of state entries written to the database. </returns>
    ///-------------------------------------------------------------------------------------------------
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Override SaveChangesAsync to automatically update timestamps. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///
    /// <param name="cancellationToken"> The cancellation token. </param>
    ///
    /// <returns>   The number of state entries written to the database. </returns>
    ///-------------------------------------------------------------------------------------------------
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Updates the timestamps for entities being added or modified. </summary>
    ///
    /// <remarks>   SvK, 01.07.2025. </remarks>
    ///-------------------------------------------------------------------------------------------------
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<SupportRequest>();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    if (entry.Entity.ReceivedAt == default)
                    {
                        entry.Entity.ReceivedAt = now;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}