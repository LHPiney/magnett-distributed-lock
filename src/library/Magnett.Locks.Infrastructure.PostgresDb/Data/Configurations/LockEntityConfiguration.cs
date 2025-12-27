using Magnett.Locks.Infrastructure.PostgresDb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Magnett.Locks.Infrastructure.PostgresDb.Data.Configurations;

public sealed class LockEntityConfiguration : IEntityTypeConfiguration<LockEntity>
{
    public void Configure(EntityTypeBuilder<LockEntity> builder)
    {
        builder.ToTable("locks", schema: "public");

        builder.HasKey(l => new { l.TenantId, l.Environment, l.Namespace, l.ResourceId })
            .HasName("PK_locks");

        builder.Property(l => l.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Tenant identifier for multi-tenancy support");

        builder.Property(l => l.Environment)
            .HasColumnName("environment")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Environment name (e.g., production, staging, development)");

        builder.Property(l => l.Namespace)
            .HasColumnName("namespace")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Logical namespace for organizing locks");

        builder.Property(l => l.ResourceId)
            .HasColumnName("resource_id")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Unique identifier of the resource being locked");

        builder.Property(l => l.LockId)
            .HasColumnName("lock_id")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Unique identifier for this specific lock instance");

        builder.Property(l => l.OwnerId)
            .HasColumnName("owner_id")
            .HasColumnType("VARCHAR(255)")
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Identifier of the process/instance that owns this lock");

        builder.Property(l => l.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("TIMESTAMP WITH TIME ZONE")
            .IsRequired()
            .HasComment("Timestamp when the lock expires (UTC)");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("TIMESTAMP WITH TIME ZONE")
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd()
            .HasComment("Timestamp when the lock was created (UTC)");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("TIMESTAMP WITH TIME ZONE")
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAddOrUpdate()
            .HasComment("Timestamp when the lock was last updated (UTC)");

        builder.HasIndex(l => l.ExpiresAt)
            .HasDatabaseName("idx_locks_expires_at")
            .HasMethod("btree")
            .HasFilter("\"expires_at\" > NOW()");

        builder.HasIndex(l => l.LockId)
            .HasDatabaseName("idx_locks_lock_id")
            .IsUnique()
            .HasMethod("btree");

        builder.HasIndex(l => new { l.TenantId, l.Environment, l.Namespace })
            .HasDatabaseName("idx_locks_tenant_env_namespace")
            .HasMethod("btree");
    }
}

