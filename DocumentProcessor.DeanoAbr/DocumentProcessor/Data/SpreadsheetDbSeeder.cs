using DocumentProcessor.Models;
using DocumentProcessor.Services;
using Microsoft.EntityFrameworkCore;

namespace DocumentProcessor.Data;

public sealed class SpreadsheetDbSeeder
{
    private readonly ICatalogReader _reader;

    public SpreadsheetDbSeeder(ICatalogReader reader) => _reader = reader;

    /// <summary>
    /// Seeds the database from the spreadsheets in <paramref name="seedDirectory"/>, but only
    /// when the database is empty. Resolves the relational graph (category names, product names,
    /// sale keys) and throws <see cref="SpreadsheetFormatException"/> on any broken reference.
    /// </summary>
    public async Task SeedIfEmptyAsync(AppDbContext db, string seedDirectory, CancellationToken ct = default)
    {
        if (await db.Products.AnyAsync(ct)) return;

        var catalog = _reader.Read(seedDirectory);

        var categories = catalog.Categories
            .Select(c => new Category { Name = c.Name, Description = c.Description })
            .ToList();
        db.Categories.AddRange(categories);
        var categoryByName = categories.ToDictionary(c => c.Name);

        var products = catalog.Products.Select(p =>
        {
            if (!categoryByName.TryGetValue(p.CategoryName, out var category))
                throw new SpreadsheetFormatException($"Product '{p.Name}' references unknown category '{p.CategoryName}'.");
            return new Product
            {
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = category,
                CreatedAtUtc = p.CreatedAtUtc,
            };
        }).ToList();
        db.Products.AddRange(products);
        var productByName = products.ToDictionary(p => p.Name);

        var sales = catalog.Sales
            .Select(s => new Sale { CustomerName = s.CustomerName, SoldAtUtc = s.SoldAtUtc })
            .ToList();
        var saleByKey = sales.ToDictionary(s => (s.CustomerName, s.SoldAtUtc));

        foreach (var item in catalog.SaleItems)
        {
            if (!saleByKey.TryGetValue((item.CustomerName, item.SoldAtUtc), out var sale))
                throw new SpreadsheetFormatException(
                    $"Sale item references unknown sale for '{item.CustomerName}' at {item.SoldAtUtc:O}.");
            if (!productByName.TryGetValue(item.ProductName, out var product))
                throw new SpreadsheetFormatException($"Sale item references unknown product '{item.ProductName}'.");

            sale.SaleItems.Add(new SaleItem
            {
                Product = product,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
            });
        }
        db.Sales.AddRange(sales);

        await db.SaveChangesAsync(ct);
    }
}
