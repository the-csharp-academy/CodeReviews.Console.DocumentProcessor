namespace DocumentProcessor.Services;

// Flat seed rows parsed from the spreadsheets. These are deliberately NOT the EF entities:
// the reader only extracts rows, and the seeder is what resolves names into object-graph
// references (category name -> Category, product name -> Product, etc.).
// Plain classes (not positional records) so CsvHelper can construct them with a default ctor
// and set each column via reflection.

public sealed class CategorySeed
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class ProductSeed
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class SaleSeed
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SoldAtUtc { get; set; }
}

public sealed class SaleItemSeed
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SoldAtUtc { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>All four entity tables read from a seed directory, in a consistent snapshot.</summary>
public sealed record SeedCatalog(
    IReadOnlyList<CategorySeed> Categories,
    IReadOnlyList<ProductSeed> Products,
    IReadOnlyList<SaleSeed> Sales,
    IReadOnlyList<SaleItemSeed> SaleItems);
