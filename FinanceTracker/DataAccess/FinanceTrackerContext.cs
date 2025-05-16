using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FinanceTracker.DataAccess;

public class FinanceTrackerContext : IdentityDbContext<FinanceUser>
{
    public FinanceTrackerContext(DbContextOptions<FinanceTrackerContext> options)
          : base(options)
    {
    }

    public DbSet<FinanceUser> Users { get; set; }
    public DbSet<WorkShift> WorkShifts { get; set; }
    public DbSet<Job> Jobs { get; set; }

    public DbSet<SupplementDetails> SupplementDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal properties with appropriate precision and scale
        modelBuilder.Entity<Job>()
            .Property(j => j.HourlyRate)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<SupplementDetails>()
            .Property(s => s.Amount)
            .HasColumnType("decimal(18,2)");
    }
}