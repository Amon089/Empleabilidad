using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Interfaces;

namespace Pqrs.Infrastructure.Persistence;

public class PqrsDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public PqrsDbContext(DbContextOptions<PqrsDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles => Set<KnowledgeBaseArticle>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketStatusHistory> TicketStatusHistories => Set<TicketStatusHistory>();
    public DbSet<RagInteraction> RagInteractions => Set<RagInteraction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        bool isNpgsql = Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        if (isNpgsql)
        {
            // PostgreSQL pgvector extension
            modelBuilder.HasPostgresExtension("vector");
        }

        // Tenant configuration
        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.Slug).IsUnique();
            builder.HasIndex(t => t.WidgetPublicKey).IsUnique();

            builder.Property(t => t.AllowedOrigins)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );
        });

        // User configuration
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            builder.Property(u => u.Role).HasConversion<string>();

            builder.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(u => !_tenantContext.HasTenant || u.TenantId == _tenantContext.TenantId);
        });

        // KnowledgeBaseArticle configuration
        modelBuilder.Entity<KnowledgeBaseArticle>(builder =>
        {
            builder.HasKey(k => k.Id);

            if (isNpgsql)
            {
                builder.Property(k => k.Embedding).HasColumnType("vector(1536)");

                // Vector HNSW Index for Npgsql
                builder.HasIndex(k => k.Embedding)
                    .HasMethod("hnsw")
                    .HasOperators("vector_cosine_ops");
            }
            else
            {
                builder.Property(k => k.Embedding)
                    .HasConversion(
                        v => v == null ? null : v.ToArray(),
                        v => v == null ? null : new Vector(v)
                    );
            }

            builder.HasIndex(k => k.TenantId);

            builder.HasOne(k => k.Tenant)
                .WithMany(t => t.Articles)
                .HasForeignKey(k => k.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(k => !_tenantContext.HasTenant || k.TenantId == _tenantContext.TenantId);
        });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Type).HasConversion<string>();
            builder.Property(t => t.Status).HasConversion<string>();
            builder.Property(t => t.Priority).HasConversion<string>();
            builder.Property(t => t.Sentiment).HasConversion<string>();

            // Relational Indexes as requested in specification
            builder.HasIndex(t => new { t.TenantId, t.Status });
            builder.HasIndex(t => new { t.TenantId, t.Priority });
            builder.HasIndex(t => new { t.TenantId, t.CreatedAt });
            builder.HasIndex(t => new { t.TenantId, t.Type });
            builder.HasIndex(t => new { t.TenantId, t.Sentiment });

            builder.HasOne(t => t.Tenant)
                .WithMany(ten => ten.Tickets)
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(t => !_tenantContext.HasTenant || t.TenantId == _tenantContext.TenantId);
        });

        // TicketStatusHistory configuration
        modelBuilder.Entity<TicketStatusHistory>(builder =>
        {
            builder.HasKey(sh => sh.Id);
            builder.Property(sh => sh.PreviousStatus).HasConversion<string>();
            builder.Property(sh => sh.NewStatus).HasConversion<string>();

            builder.HasOne(sh => sh.Ticket)
                .WithMany(t => t.StatusHistory)
                .HasForeignKey(sh => sh.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(sh => !_tenantContext.HasTenant || sh.TenantId == _tenantContext.TenantId);
        });

        // RagInteraction configuration
        modelBuilder.Entity<RagInteraction>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => new { r.TenantId, r.CreatedAt });

            builder.HasQueryFilter(r => !_tenantContext.HasTenant || r.TenantId == _tenantContext.TenantId);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => new { a.TenantId, a.CreatedAt });

            builder.HasQueryFilter(a => !_tenantContext.HasTenant || a.TenantId == _tenantContext.TenantId);
        });
    }
}
