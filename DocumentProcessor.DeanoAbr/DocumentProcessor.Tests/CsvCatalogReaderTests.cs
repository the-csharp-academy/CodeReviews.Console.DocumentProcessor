using DocumentProcessor.Services;
using Xunit;

namespace DocumentProcessor.Tests;

public sealed class CsvCatalogReaderTests
{
    private const string Categories =
        "Name,Description\n" +
        "Coffee,Single-origin and blended\n" +
        "Brewing Equipment,Grinders and presses\n";

    private const string Products =
        "Name,Description,Price,StockQuantity,Category,CreatedAtUtc\n" +
        "Espresso House Blend 250g,Signature dark roast,69.00,75,Coffee,2026-01-08T08:00:00Z\n" +
        "French Press 600ml,Borosilicate glass,429.00,11,Brewing Equipment,2026-01-12T08:00:00Z\n";

    private const string Sales =
        "CustomerName,SoldAtUtc\n" +
        "Amara Nkosi,2026-02-03T09:15:00Z\n";

    private const string SaleItems =
        "CustomerName,SoldAtUtc,ProductName,Quantity\n" +
        "Amara Nkosi,2026-02-03T09:15:00Z,Espresso House Blend 250g,2\n";

    private static string WriteSeedDir(string categories, string products, string sales, string saleItems)
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "categories.csv"), categories);
        File.WriteAllText(Path.Combine(dir, "products.csv"), products);
        File.WriteAllText(Path.Combine(dir, "sales.csv"), sales);
        File.WriteAllText(Path.Combine(dir, "sale_items.csv"), saleItems);
        return dir;
    }

    private static void DeleteDir(string dir)
    {
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
    }

    [Fact]
    public void Read_ValidSeed_ReturnsAllFourTables()
    {
        var dir = WriteSeedDir(Categories, Products, Sales, SaleItems);

        try
        {
            var catalog = new CsvCatalogReader().Read(dir);

            Assert.Equal(2, catalog.Categories.Count);
            Assert.Equal(2, catalog.Products.Count);
            Assert.Single(catalog.Sales);
            Assert.Single(catalog.SaleItems);

            var product = catalog.Products[0];
            Assert.Equal("Espresso House Blend 250g", product.Name);
            Assert.Equal(69.00m, product.Price);
            Assert.Equal("Coffee", product.CategoryName);
            Assert.Equal(new DateTime(2026, 1, 8, 8, 0, 0, DateTimeKind.Utc), product.CreatedAtUtc);

            var saleItem = catalog.SaleItems[0];
            Assert.Equal("Espresso House Blend 250g", saleItem.ProductName);
            Assert.Equal(2, saleItem.Quantity);
        }
        finally
        {
            DeleteDir(dir);
        }
    }

    [Fact]
    public void Read_MissingDirectory_ThrowsSpreadsheetFormatException()
    {
        var ex = Assert.Throws<SpreadsheetFormatException>(
            () => new CsvCatalogReader().Read("Z:\\does\\not\\exist"));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Read_MissingFile_ThrowsSpreadsheetFormatException()
    {
        var dir = WriteSeedDir(Categories, Products, Sales, SaleItems);
        File.Delete(Path.Combine(dir, "products.csv"));

        try
        {
            var ex = Assert.Throws<SpreadsheetFormatException>(() => new CsvCatalogReader().Read(dir));
            Assert.Contains("products.csv", ex.Message);
        }
        finally
        {
            DeleteDir(dir);
        }
    }

    [Fact]
    public void Read_MalformedPrice_ThrowsSpreadsheetFormatException()
    {
        var badProducts =
            "Name,Description,Price,StockQuantity,Category,CreatedAtUtc\n" +
            "Broken,no price,not-a-number,1,Coffee,2026-01-08T08:00:00Z\n";
        var dir = WriteSeedDir(Categories, badProducts, Sales, SaleItems);

        try
        {
            Assert.Throws<SpreadsheetFormatException>(() => new CsvCatalogReader().Read(dir));
        }
        finally
        {
            DeleteDir(dir);
        }
    }
}
