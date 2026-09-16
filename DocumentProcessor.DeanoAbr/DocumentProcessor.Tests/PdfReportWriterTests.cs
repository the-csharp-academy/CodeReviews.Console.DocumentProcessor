using DocumentProcessor.Models;
using DocumentProcessor.Services;
using Xunit;

namespace DocumentProcessor.Tests;

public sealed class PdfReportWriterTests
{
    [Fact]
    public void Write_CreatesNonEmptyPdfFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        var products = new[]
        {
            new Product { Name = "Espresso", Description = "Dark roast", Price = 69m, StockQuantity = 5, Category = new Category { Name = "Coffee" } },
        };
        var sales = new[]
        {
            new Sale
            {
                CustomerName = "Amara Nkosi",
                SoldAtUtc = new DateTime(2026, 2, 3, 9, 15, 0, DateTimeKind.Utc),
                SaleItems = { new SaleItem { ProductName = "Espresso", Quantity = 2, UnitPrice = 69m } },
            },
        };

        try
        {
            new PdfReportWriter().Write(products, sales, path);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
