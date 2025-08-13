using Microsoft.EntityFrameworkCore;
using warehouse_api.Models;

namespace warehouse_api.Data;

public class WarehouseContext : DbContext
{
    public WarehouseContext(DbContextOptions<WarehouseContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Arrival> Arrivals { get; set; }
    public DbSet<ArrivalResource> ArrivalResources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Unit>().HasIndex(u => u.Name).IsUnique();
        modelBuilder.Entity<Arrival>().HasIndex(a => a.Number).IsUnique();
    }
}
