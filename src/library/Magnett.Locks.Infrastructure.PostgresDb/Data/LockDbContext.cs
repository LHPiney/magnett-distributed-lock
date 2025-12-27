using Magnett.Locks.Infrastructure.PostgresDb.Data.Configurations;
using Magnett.Locks.Infrastructure.PostgresDb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magnett.Locks.Infrastructure.PostgresDb.Data;

public sealed class LockDbContext : DbContext
{
    public LockDbContext(DbContextOptions<LockDbContext> options) : base(options)
    {
    }

    public DbSet<LockEntity> Locks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LockEntityConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

