using Microsoft.EntityFrameworkCore;
using mini_mes_be.Models;
using mini_mes_be.Extensions;

namespace mini_mes_be.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.id);
            e.Property(u => u.id).ValueGeneratedOnAdd();        // IDENTITY(1,1)
            e.HasIndex(u => u.username).IsUnique();
            e.HasIndex(u => u.email).IsUnique();
            e.Property(u => u.username).HasMaxLength(100).IsRequired();
            e.Property(u => u.email).HasMaxLength(256).IsRequired();
            e.Property(u => u.role).HasMaxLength(50).IsRequired();
        });

        // ── RefreshToken ──────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.id);
            e.Property(rt => rt.id).ValueGeneratedOnAdd();      // IDENTITY(1,1)
            e.HasIndex(rt => rt.token).IsUnique();
            e.HasOne(rt => rt.user)
             .WithMany(u => u.refresh_tokens)
             .HasForeignKey(rt => rt.user_id)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Apply snake_case naming convention to columns, keys, foreign keys, and indexes
        // Note: keep table names in PascalCase (e.g. Users, RefreshTokens) per DB requirement.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Column names
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToSnakeCase());
            }

            // Keys
            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (keyName != null)
                {
                    key.SetName(keyName.ToSnakeCase());
                }
            }

            // Foreign Keys
            foreach (var fk in entity.GetForeignKeys())
            {
                var fkName = fk.GetConstraintName();
                if (fkName != null)
                {
                    fk.SetConstraintName(fkName.ToSnakeCase());
                }
            }

            // Indexes
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (indexName != null)
                {
                    index.SetDatabaseName(indexName.ToSnakeCase());
                }
            }
        }
    }
}
