using DocumentProcessor.Data;
using DocumentProcessor.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocumentProcessor.Tests;

public sealed class SpreadsheetDbSeederTests
{
    private static (AppDbContext db, SqliteConnection conn) CreateDb()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return (db, conn);
    }

    private sealed class FakeReader : ICatalogReader
    {
        private readonly SeedCatalog _catalog;
        public int CallCount { get; private set; }
        public FakeReader(SeedCatalog catalog) => _catalog = catalog;
        public SeedCatalog Read(string seedDirectory) { CallCount++; return _catalog; }
    }

    private static SeedCatalog ValidCatalog() => new(
        new CategorySeed[]
        {
            new() { Name = "Coffee", Description = "Roasted beans" },
            new() { Name = "Brewing Equipment", Description = "Grinders and presses" },
        },
        new ProductSeed[]
        {
            new() { Name = "Espresso", Description = "Dark roast", Price = 69.00m, StockQuantity = 75, CategoryName = "Coffee", CreatedAtUtc = new DateTime(2026, 1, 8, 8, 0, 0, DateTimeKind.Utc) },
            new() { Name = "French Press", Description = "Glass and steel", Price = 429.00m, StockQuantity = 11, CategoryName = "Brewing Equipment", CreatedAtUtc = new DateTime(2026, 1, 12, 8, 0, 0, DateTimeKind.Utc) },
        },
        new SaleSeed[]
        {
            new() { CustomerName = "Amara Nkosi", SoldAtUtc = new DateTime(2026, 2, 3, 9, 15, 0, DateTimeKind.Utc) },
        },
        new SaleItemSeed[]
        {
            new() { CustomerName = "Amara Nkosi", SoldAtUtc = new DateTime(2026, 2, 3, 9, 15, 0, DateTimeKind.Utc), ProductName = "Espresso", Quantity = 2 },
            new() { CustomerName = "Amara Nkosi", SoldAtUtc = new DateTime(2026, 2, 3, 9, 15, 0, DateTimeKind.Utc), ProductName = "French Press", Quantity = 1 },
        });

    [Fact]
    public async Task SeedIfEmptyAsync_EmptyDb_SeedsFullGraph()
    {
        var (db, conn) = CreateDb();
        using (db) using (conn)
        {
            var reader = new FakeReader(ValidCatalog());
            var seeder = new SpreadsheetDbSeeder(reader);

            await seeder.SeedIfEmptyAsync(db, "ignored");

            Assert.Equal(1, reader.CallCount);
            Assert.Equal(2, await db.Categories.CountAsync());
            Assert.Equal(2, await db.Products.CountAsync());
            Assert.Equal(1, await db.Sales.CountAsync());
            Assert.Equal(2, await db.SaleItems.CountAsync());

            // Category navigation wired by name.
            var espresso = await db.Products.Include(p => p.Category).SingleAsync(p => p.Name == "Espresso");
            Assert.Equal("Coffee", espresso.Category!.Name);

            // Sale total computed from the two receipt-snapshot lines: 2*69 + 1*429 = 567.
            var sale = await db.Sales.Include(s => s.SaleItems).SingleAsync();
            Assert.Equal(567.00m, sale.Total);
        }
    }

    [Fact]
    public async Task SeedIfEmptyAsync_PopulatedDb_DoesNotRead()
    {
        var (db, conn) = CreateDb();
        using (db) using (conn)
        {
            db.Products.Add(new Models.Product { Name = "Existing", Price = 1m, StockQuantity = 1, Category = new Models.Category { Name = "X" } });
            await db.SaveChangesAsync();

            var reader = new FakeReader(ValidCatalog());
            var seeder = new SpreadsheetDbSeeder(reader);

            await seeder.SeedIfEmptyAsync(db, "ignored");

            Assert.Equal(0, reader.CallCount);
            Assert.Equal(1, await db.Products.CountAsync());
        }
    }

    [Fact]
    public async Task SeedIfEmptyAsync_UnknownProductReference_ThrowsSpreadsheetFormatException()
    {
        var (db, conn) = CreateDb();
        using (db) using (conn)
        {
            var broken = new SeedCatalog(
                ValidCatalog().Categories,
                ValidCatalog().Products,
                ValidCatalog().Sales,
                new SaleItemSeed[]
                {
                    new() { CustomerName = "Amara Nkosi", SoldAtUtc = new DateTime(2026, 2, 3, 9, 15, 0, DateTimeKind.Utc), ProductName = "Ghost Product", Quantity = 1 },
                });

            var reader = new FakeReader(broken);
            var seeder = new SpreadsheetDbSeeder(reader);

            var ex = await Assert.ThrowsAsync<SpreadsheetFormatException>(
                () => seeder.SeedIfEmptyAsync(db, "ignored"));

            Assert.Contains("unknown product", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
