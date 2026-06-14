using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Infrastructure.Data;

public class OciDbContext : DbContext
{
    public OciDbContext(DbContextOptions<OciDbContext> options) : base(options) { }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Vuln> Vulns => Set<Vuln>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Asset>()
            .HasMany(a => a.AssetVulns)
            .WithOne(av => av.Asset)
            .HasForeignKey(av => av.AssetId);

        modelBuilder.Entity<Vuln>()
            .HasMany(v => v.AssetVulns)
            .WithOne(av => av.Vuln)
            .HasForeignKey(av => av.VulnId);
    }
}