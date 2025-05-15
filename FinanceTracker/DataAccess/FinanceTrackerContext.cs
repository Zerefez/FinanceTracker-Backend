using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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
        
        // Specify decimal precision for PostgreSQL
        if (_env.IsProduction())
        {
            // Apply PostgreSQL-specific configurations
            modelBuilder.Entity<SupplementDetails>()
                .Property(s => s.Amount)
                .HasPrecision(18, 2);
                
            modelBuilder.Entity<Job>()
                .Property(j => j.HourlyRate)
                .HasPrecision(18, 2);
        }
    }
}