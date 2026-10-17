using Microsoft.EntityFrameworkCore;
using RideService.model;

public class RideContext : DbContext
{
    public DbSet<Ride> Rides { get; set; }

    public RideContext(DbContextOptions<RideContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // store the status as text ("Requested") so the table is readable in psql
        modelBuilder.Entity<Ride>()
            .Property(e => e.Status)
            .HasConversion<string>();

        // maps Version to Postgres' built-in xmin column (optimistic concurrency)
        modelBuilder.Entity<Ride>()
            .Property(e => e.Version)
            .IsRowVersion();

        // GET /rides/available filters on Status, so index it
        modelBuilder.Entity<Ride>()
            .HasIndex(e => e.Status);
    }
}
