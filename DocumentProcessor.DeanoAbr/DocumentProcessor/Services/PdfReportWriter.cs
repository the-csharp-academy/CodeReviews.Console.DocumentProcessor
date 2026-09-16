using DocumentProcessor.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DocumentProcessor.Services;

public sealed class PdfReportWriter : IPdfReportWriter
{
    public void Write(IReadOnlyList<Product> products, IReadOnlyList<Sale> sales, string outputPath)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.Header().Text("Bean & Barrel — Catalogue & Sales Report").FontSize(18).Bold();

            page.Content().PaddingTop(10).Column(column =>
            {
                column.Item().Text("Products").FontSize(14).Bold();
                column.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Name
                        cols.RelativeColumn(3); // Description
                        cols.RelativeColumn(1); // Price
                        cols.RelativeColumn(1); // Stock
                        cols.RelativeColumn(2); // Category
                    });

                    table.Header(h =>
                    {
                        foreach (var title in new[] { "Name", "Description", "Price", "Stock", "Category" })
                            h.Cell().Element(CellStyle).Text(title).Bold();
                    });

                    foreach (var p in products)
                    {
                        table.Cell().Element(CellStyle).Text(p.Name);
                        table.Cell().Element(CellStyle).Text(p.Description);
                        table.Cell().Element(CellStyle).Text($"R {p.Price:0.00}");
                        table.Cell().Element(CellStyle).Text(p.StockQuantity.ToString());
                        table.Cell().Element(CellStyle).Text(p.Category?.Name ?? string.Empty);
                    }
                });

                column.Item().PaddingTop(16).Text("Sales").FontSize(14).Bold();
                column.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2); // Customer
                        cols.RelativeColumn(2); // Date
                        cols.RelativeColumn(1); // Items
                        cols.RelativeColumn(1); // Total
                    });

                    table.Header(h =>
                    {
                        foreach (var title in new[] { "Customer", "Date", "Items", "Total" })
                            h.Cell().Element(CellStyle).Text(title).Bold();
                    });

                    foreach (var s in sales)
                    {
                        table.Cell().Element(CellStyle).Text(s.CustomerName);
                        table.Cell().Element(CellStyle).Text(s.SoldAtUtc.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(s.SaleItems.Count.ToString());
                        table.Cell().Element(CellStyle).Text($"R {s.Total:0.00}");
                    }
                });
            });

            page.Footer().AlignRight().Text(t => t.CurrentPageNumber());
        })).GeneratePdf(outputPath);
    }

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
}
