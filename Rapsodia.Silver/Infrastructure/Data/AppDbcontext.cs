// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Domain.Models;
using Rapsodia.Silver.Infrastructure.Security;

namespace Rapsodia.Silver.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext>? _logger;
    private readonly string _schema;

    public AppDbContext(DbContextOptions<AppDbContext> options, string schema = "principal")
        : base(options)
    {
        _schema = schema;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext>? logger, string schema = "principal")
        : base(options)
    {
        _schema = schema;
        _logger = logger;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Telemetry> Telemetries => Set<Telemetry>();
    public DbSet<SecurityAnalysis> SecurityAnalyses => Set<SecurityAnalysis>();
    public DbSet<GraphEdge> GraphEdges => Set<GraphEdge>();
    public DbSet<EntropyHistory> EntropyHistories => Set<EntropyHistory>();
    public DbSet<ConversationHistory> ConversationHistories => Set<ConversationHistory>();
    public DbSet<GraphHistory> GraphHistories => Set<GraphHistory>();
    public DbSet<AgentMetadata> AgentMetadata => Set<AgentMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_schema);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
        modelBuilder.Entity<Telemetry>().HasQueryFilter(t => t.DeletedAt == null);

        modelBuilder.Entity<GraphEdge>(b =>
        {
            b.ToTable("GraphEdges");
            b.HasKey(e => new { e.SourceId, e.TargetId, e.RelationType });
            b.Property(e => e.OriginType).HasMaxLength(50).IsRequired();
            b.Property(e => e.TargetType).HasMaxLength(50).IsRequired();
            b.Property(e => e.RelationType).HasMaxLength(100).IsRequired();
            b.HasIndex(e => e.SourceId);
            b.HasIndex(e => e.TargetId);
        });

        modelBuilder.Entity<GraphHistory>().ToTable("GraphHistories");
        modelBuilder.Entity<AgentMetadata>(b =>
        {
            b.ToTable("AgentMetadata");
            b.HasIndex(a => a.AgentId).IsUnique();
            b.HasQueryFilter(a => a.DeletedAt == null);
        });

        ConfigureFieldEncryption(modelBuilder);
    }

    private void ConfigureFieldEncryption(ModelBuilder modelBuilder)
    {
        var encryptionKey = Environment.GetEnvironmentVariable("CRYPT_FLD");

        if (string.IsNullOrWhiteSpace(encryptionKey))
            throw new InvalidOperationException("Chave CRYPT_FLD obrigatoria nao configurada.");

        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(encryptionKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("CRYPT_FLD deve estar em Base64 valido.", ex);
        }

        if (keyBytes.Length != 32)
            throw new InvalidOperationException("CRYPT_FLD deve ter exatamente 32 bytes.");

        var aesConverter = new ValueConverter<string, string>(
            plaintext => AesGcmHelper.Encrypt(plaintext, keyBytes),
            ciphertext => AesGcmHelper.Decrypt(ciphertext, keyBytes)
        );

        modelBuilder.Entity<Telemetry>()
            .Property(t => t.TargetFilePath)
            .HasConversion(aesConverter);
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
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = now;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                    break;
            }
        }
    }
}