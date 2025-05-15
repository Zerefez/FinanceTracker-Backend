using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FinanceTracker.DataAccess;

public class FinanceTrackerContext : IdentityDbContext<FinanceUser>
{
    private readonly IHostEnvironment _env;

    public FinanceTrackerContext(DbContextOptions<FinanceTrackerContext> options, IHostEnvironment env)
          : base(options)
    {
        _env = env;
    }

    public DbSet<FinanceUser> Users { get; set; }
    public DbSet<WorkShift> WorkShifts { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<SupplementDetails> SupplementDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Explicitly configure table names
        modelBuilder.Entity<Job>().ToTable("Jobs");
        modelBuilder.Entity<WorkShift>().ToTable("WorkShifts");
        modelBuilder.Entity<SupplementDetails>().ToTable("SupplementDetails");
        
        // Specify decimal precision for all tables
        modelBuilder.Entity<SupplementDetails>()
            .Property(s => s.Amount)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Job>()
            .Property(j => j.HourlyRate)
            .HasPrecision(18, 2);
            
        // Configure composite keys
        modelBuilder.Entity<Job>()
            .HasKey(j => new { j.CompanyName, j.UserId });
            
        modelBuilder.Entity<WorkShift>()
            .HasKey(w => new { w.StartTime, w.EndTime, w.UserId });
            
        modelBuilder.Entity<SupplementDetails>()
            .HasKey(s => new { s.Weekday, s.CompanyName });
        
        if (_env.IsProduction())
        {
            // Apply PostgreSQL-specific configurations
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Replace SQL Server IDENTITY with PostgreSQL SERIAL
                foreach (var property in entity.GetProperties())
                {
                    if (property.GetAnnotations()
                        .Any(a => a.Name == "SqlServer:ValueGenerationStrategy" && 
                                 (int)a.Value == 1)) // SqlServerValueGenerationStrategy.IdentityColumn
                    {
                        property.SetAnnotation("Npgsql:ValueGenerationStrategy", 
                            NpgsqlValueGenerationStrategy.SerialColumn);
                    }
                }
            }
        }
    }
}