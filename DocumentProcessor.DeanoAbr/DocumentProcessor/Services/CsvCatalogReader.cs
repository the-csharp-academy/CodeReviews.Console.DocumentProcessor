using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DocumentProcessor.Services;

public interface ICatalogReader
{
    /// <summary>
    /// Reads the four seed files (categories.csv, products.csv, sales.csv, sale_items.csv)
    /// from the given directory and returns them as a single snapshot.
    /// </summary>
    SeedCatalog Read(string seedDirectory);
}

public sealed class CsvCatalogReader : ICatalogReader
{
    public SeedCatalog Read(string seedDirectory)
    {
        if (!Directory.Exists(seedDirectory))
            throw new SpreadsheetFormatException($"Seed directory not found: {seedDirectory}");

        try
        {
            return new SeedCatalog(
                Read<CategorySeed>(seedDirectory, "categories.csv", new CategoryMap()),
                Read<ProductSeed>(seedDirectory, "products.csv", new ProductMap()),
                Read<SaleSeed>(seedDirectory, "sales.csv", new SaleMap()),
                Read<SaleItemSeed>(seedDirectory, "sale_items.csv", new SaleItemMap()));
        }
        catch (SpreadsheetFormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SpreadsheetFormatException($"Failed to read seed data from '{seedDirectory}': {ex.Message}", ex);
        }
    }

    private static IReadOnlyList<T> Read<T>(string seedDirectory, string fileName, ClassMap<T> map)
    {
        var path = Path.Combine(seedDirectory, fileName);
        if (!File.Exists(path))
            throw new SpreadsheetFormatException($"Seed file not found: {path}");

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap(map);
        return csv.GetRecords<T>().ToList();
    }

    private sealed class CategoryMap : ClassMap<CategorySeed>
    {
        public CategoryMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Description).Name("Description");
        }
    }

    private sealed class ProductMap : ClassMap<ProductSeed>
    {
        public ProductMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Description).Name("Description");
            Map(m => m.Price).Name("Price").TypeConverterOption.NumberStyles(NumberStyles.Number);
            Map(m => m.StockQuantity).Name("StockQuantity");
            Map(m => m.CategoryName).Name("Category");
            Map(m => m.CreatedAtUtc).Name("CreatedAtUtc")
                .TypeConverterOption.DateTimeStyles(DateTimeStyles.AdjustToUniversal);
        }
    }

    private sealed class SaleMap : ClassMap<SaleSeed>
    {
        public SaleMap()
        {
            Map(m => m.CustomerName).Name("CustomerName");
            Map(m => m.SoldAtUtc).Name("SoldAtUtc")
                .TypeConverterOption.DateTimeStyles(DateTimeStyles.AdjustToUniversal);
        }
    }

    private sealed class SaleItemMap : ClassMap<SaleItemSeed>
    {
        public SaleItemMap()
        {
            Map(m => m.CustomerName).Name("CustomerName");
            Map(m => m.SoldAtUtc).Name("SoldAtUtc")
                .TypeConverterOption.DateTimeStyles(DateTimeStyles.AdjustToUniversal);
            Map(m => m.ProductName).Name("ProductName");
            Map(m => m.Quantity).Name("Quantity");
        }
    }
}
