using Microsoft.EntityFrameworkCore;
using mini_mes_be.Models;
using mini_mes_be.Extensions;

namespace mini_mes_be.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderLog> WorkOrderLogs => Set<WorkOrderLog>();
    public DbSet<WorkOrderProduct> WorkOrderProducts => Set<WorkOrderProduct>();
    public DbSet<QualityCheck> QualityChecks => Set<QualityCheck>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<BomItem> BomItems => Set<BomItem>();

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

        // ── Product ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.id);
            e.Property(p => p.id).ValueGeneratedOnAdd();
            e.HasIndex(p => p.sku).IsUnique();
            e.Property(p => p.name).HasMaxLength(200).IsRequired();
            e.Property(p => p.sku).HasMaxLength(100).IsRequired();
            e.Property(p => p.unit).HasMaxLength(50).IsRequired();
            e.Property(p => p.description).HasMaxLength(1000);
        });

        // ── Machine ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Machine>(e =>
        {
            e.HasKey(m => m.id);
            e.Property(m => m.id).ValueGeneratedOnAdd();
            e.HasIndex(m => m.code).IsUnique();
            e.Property(m => m.name).HasMaxLength(200).IsRequired();
            e.Property(m => m.code).HasMaxLength(50).IsRequired();
            e.Property(m => m.location).HasMaxLength(200);
            e.Property(m => m.status)
             .HasConversion<string>()
             .HasMaxLength(50);
        });

        // ── WorkOrder ────────────────────────────────────────────────────────
        modelBuilder.Entity<WorkOrder>(e =>
        {
            e.HasKey(wo => wo.id);
            e.Property(wo => wo.id).ValueGeneratedOnAdd();
            e.HasIndex(wo => wo.order_number).IsUnique();
            e.Property(wo => wo.order_number).HasMaxLength(50).IsRequired();
            e.Property(wo => wo.status)
             .HasConversion<string>()
             .HasMaxLength(50);
            e.Property(wo => wo.notes).HasMaxLength(1000);

            e.HasOne(wo => wo.machine)
             .WithMany(m => m.work_orders)
             .HasForeignKey(wo => wo.machine_id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(wo => wo.created_by_user)
             .WithMany()
             .HasForeignKey(wo => wo.created_by_user_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WorkOrderProduct ──────────────────────────────────────────────────
        modelBuilder.Entity<WorkOrderProduct>(e =>
        {
            e.HasKey(wop => wop.id);
            e.Property(wop => wop.id).ValueGeneratedOnAdd();

            e.HasOne(wop => wop.work_order)
             .WithMany(wo => wo.products)
             .HasForeignKey(wop => wop.work_order_id)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(wop => wop.product)
             .WithMany(p => p.work_order_products)
             .HasForeignKey(wop => wop.product_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── WorkOrderLog ─────────────────────────────────────────────────────
        modelBuilder.Entity<WorkOrderLog>(e =>
        {
            e.HasKey(wl => wl.id);
            e.Property(wl => wl.id).ValueGeneratedOnAdd();
            e.Property(wl => wl.event_type).HasMaxLength(50).IsRequired();
            e.Property(wl => wl.message).HasMaxLength(500).IsRequired();
            e.Property(wl => wl.old_value).HasMaxLength(100);
            e.Property(wl => wl.new_value).HasMaxLength(100);

            e.HasOne(wl => wl.work_order)
             .WithMany(wo => wo.logs)
             .HasForeignKey(wl => wl.work_order_id)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(wl => wl.user)
             .WithMany()
             .HasForeignKey(wl => wl.user_id)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── QualityCheck ─────────────────────────────────────────────────────
        modelBuilder.Entity<QualityCheck>(e =>
        {
            e.HasKey(qc => qc.id);
            e.Property(qc => qc.id).ValueGeneratedOnAdd();
            e.Property(qc => qc.result)
             .HasConversion<string>()
             .HasMaxLength(50);
            e.Property(qc => qc.notes).HasMaxLength(1000);

            e.HasOne(qc => qc.work_order)
             .WithMany(wo => wo.quality_checks)
             .HasForeignKey(qc => qc.work_order_id)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(qc => qc.product)
             .WithMany()
             .HasForeignKey(qc => qc.product_id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(qc => qc.inspector)
             .WithMany()
             .HasForeignKey(qc => qc.inspector_user_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── InventoryTransaction ─────────────────────────────────────────────
        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.HasKey(it => it.id);
            e.Property(it => it.id).ValueGeneratedOnAdd();
            e.Property(it => it.type)
             .HasConversion<string>()
             .HasMaxLength(50);
            e.Property(it => it.reference).HasMaxLength(500);

            e.HasOne(it => it.product)
             .WithMany(p => p.inventory_transactions)
             .HasForeignKey(it => it.product_id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(it => it.work_order)
             .WithMany()
             .HasForeignKey(it => it.work_order_id)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(it => it.user)
             .WithMany()
             .HasForeignKey(it => it.user_id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── BomItem ──────────────────────────────────────────────────────────
        modelBuilder.Entity<BomItem>(e =>
        {
            e.HasKey(b => b.id);
            e.Property(b => b.id).ValueGeneratedOnAdd();

            // A product can only list each material once
            e.HasIndex(b => new { b.product_id, b.material_id }).IsUnique();

            e.Property(b => b.quantity).HasPrecision(18, 4);
            e.Property(b => b.unit).HasMaxLength(50).IsRequired();
            e.Property(b => b.notes).HasMaxLength(500);

            e.HasOne(b => b.product)
             .WithMany(p => p.bom_items)
             .HasForeignKey(b => b.product_id)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.material)
             .WithMany(p => p.bom_used_in)
             .HasForeignKey(b => b.material_id)
             .OnDelete(DeleteBehavior.Restrict);
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
