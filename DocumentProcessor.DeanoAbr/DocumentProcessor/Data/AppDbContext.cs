using DocumentProcessor.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentProcessor.Data;

public class AppDbContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Description).HasMaxLength(500);
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.CreatedAtUtc).IsRequired();
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(s => s.CustomerName).IsRequired().HasMaxLength(100);
            e.Property(s => s.SoldAtUtc).IsRequired();
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.Property(si => si.ProductName).IsRequired().HasMaxLength(200);
            e.Property(si => si.UnitPrice).HasPrecision(18, 2);
            e.HasOne(si => si.Sale).WithMany(s => s.SaleItems).HasForeignKey(si => si.SaleId);
            e.HasOne(si => si.Product).WithMany(p => p.SaleItems).HasForeignKey(si => si.ProductId);
        });
    }
}
