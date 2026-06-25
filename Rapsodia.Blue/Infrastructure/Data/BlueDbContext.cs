// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Domain.Entities.Olimpo;
using Rapsodia.Blue.Domain.Enums;
using Rapsodia.Blue.Infrastructure.Data.Providers;

namespace Rapsodia.Blue.Infrastructure.Data;

public class BlueDbContext : DbContext
{
    public BlueDbContext(DbContextOptions<BlueDbContext> options) : base(options) { }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();
    public DbSet<Vuln> Vulns => Set<Vuln>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AssetVuln> AssetVulns => Set<AssetVuln>();
    public DbSet<OlimpoCredential> OlimpoCredentials => Set<OlimpoCredential>();
    public DbSet<OlimpoTotpAccount> OlimpoTotpAccounts => Set<OlimpoTotpAccount>();
    public DbSet<OlimpoDocument> OlimpoDocuments => Set<OlimpoDocument>();
    public DbSet<SyncQueue> SyncQueues => Set<SyncQueue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(a => a.Property(x => x.Id).ValueGeneratedOnAdd());
        modelBuilder.Entity<Vuln>(v => v.Property(x => x.Id).ValueGeneratedOnAdd());
        modelBuilder.Entity<User>(u => u.Property(x => x.Id).ValueGeneratedOnAdd());
        modelBuilder.Entity<SyncQueue>(s => s.Property(x => x.Id).ValueGeneratedOnAdd());
        modelBuilder.Entity<AssetType>(t => t.Property(x => x.Id).ValueGeneratedOnAdd());

        modelBuilder.Entity<Asset>(a =>
        {
            a.HasQueryFilter(x => x.DeletedAt == null);
            a.HasOne(x => x.ParentAsset)
             .WithMany(x => x.ChildAssets)
             .HasForeignKey(x => x.ParentAssetId)
             .OnDelete(DeleteBehavior.Restrict);
            a.HasMany(x => x.RelatedAssets)
             .WithMany()
             .UsingEntity("AssetAsset");
            a.HasOne(x => x.AssetType)
             .WithMany()
             .HasForeignKey(x => x.AssetTypeId);
        });

        modelBuilder.Entity<Vuln>(v =>
        {
            v.HasQueryFilter(x => x.DeletedAt == null);
            v.HasOne(x => x.ParentVuln)
             .WithMany(x => x.ChildVulns)
             .HasForeignKey(x => x.ParentVulnId)
             .OnDelete(DeleteBehavior.Restrict);
            v.HasMany(x => x.RelatedVulns)
             .WithMany()
             .UsingEntity("VulnVuln");
        });

        modelBuilder.Entity<AssetVuln>(av =>
        {
            av.HasKey(x => new { x.AssetId, x.VulnId });
            av.HasQueryFilter(x => x.Asset.DeletedAt == null && x.Vuln.DeletedAt == null); 
            av.HasOne(x => x.Asset).WithMany(a => a.AssetVulns).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            av.HasOne(x => x.Vuln).WithMany(v => v.AssetVulns).HasForeignKey(x => x.VulnId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        modelBuilder.Entity<OlimpoCredential>(entity =>
        {
            entity.ToTable("OLP_Credentials");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(256);
            entity.Property(e => e.EncryptedValue).IsRequired();
            entity.Property(e => e.Environment).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Category).HasMaxLength(128);
            entity.HasIndex(e => new { e.UserId, e.Key, e.Environment }).IsUnique();
        });

        modelBuilder.Entity<OlimpoTotpAccount>(entity =>
        {
            entity.ToTable("OLP_TotpAccounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Uri).IsRequired();
            entity.Property(e => e.Secret).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Label).HasMaxLength(256);
            entity.Property(e => e.Issuer).HasMaxLength(256);
            entity.Property(e => e.Algorithm).HasMaxLength(16);
            entity.HasIndex(e => new { e.UserId, e.Issuer, e.Label }).IsUnique();
        });

        modelBuilder.Entity<OlimpoDocument>(entity =>
        {
            entity.ToTable("OLP_Documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(512);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ContentType).HasMaxLength(256);
            entity.Property(e => e.StoragePath).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(128);
        });

        modelBuilder.Entity<SyncQueue>(entity =>
        {
            entity.ToTable("SYS_SyncQueue");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Operation).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.LastError).HasMaxLength(1024);
            entity.HasIndex(e => e.Status);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SyncSaveChangesInterceptor());
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAudit();
        return await base.SaveChangesAsync(ct);
    }

    private void ApplyAudit()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = now;
                    break;
            }
        }
    }
}