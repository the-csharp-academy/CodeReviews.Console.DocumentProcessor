using kilozdazolik.DocumentProcessor.Data;
using kilozdazolik.DocumentProcessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace kilozdazolik.DocumentProcessor.Services
{
    public class ReportService(PhoneBookContext context, ILogger<ReportService> logger) : IReportService
    {
        public async Task ExportToPdfAsync(string filePath)
        {
            var contacts = await context.Contacts
                .Include(c => c.Category)
                .OrderBy(c => c.Category.Name)
                .ThenBy(c => c.Name)
                .ToListAsync();

            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, contacts));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(filePath);

            logger.LogInformation("PDF report generated at {FilePath} with {Count} contacts.", filePath, contacts.Count);
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PhoneBook Report")
                        .FontSize(20).Bold().FontColor(Colors.Teal.Medium);
                    col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, List<Models.Contact> contacts)
        {
            container.PaddingTop(10).Column(col =>
            {
                col.Item().Text($"Total contacts: {contacts.Count}")
                    .FontSize(10).FontColor(Colors.Grey.Darken2).Italic();

                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Name
                        cols.RelativeColumn(4); // Email
                        cols.RelativeColumn(2); // Phone
                        cols.RelativeColumn(2); // Category
                    });

                    // Header
                    table.Header(header =>
                    {
                        foreach (var title in new[] { "Name", "Email", "Phone", "Category" })
                        {
                            header.Cell().Background(Colors.Teal.Medium).Padding(5)
                                .Text(title).FontColor(Colors.White).Bold();
                        }
                    });

                    // Rows
                    var isAlternate = false;
                    foreach (var contact in contacts)
                    {
                        var bg = isAlternate ? Colors.Grey.Lighten3 : Colors.White;
                        isAlternate = !isAlternate;

                        foreach (var value in new[] { contact.Name, contact.Email ?? "-", contact.PhoneNumber, contact.Category?.Name ?? "-" })
                        {
                            table.Cell().Background(bg).Padding(5).Text(value);
                        }
                    }
                });
            });
        }
    }
}