using DocumentProcessor.Data;
using DocumentProcessor.Services;
using Microsoft.EntityFrameworkCore;

var seedDirectory = ResolveSeedPath(args);

var dbPath = "documentprocessor.db";
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var db = new AppDbContext(options);
db.Database.EnsureCreated();

var seeder = new SpreadsheetDbSeeder(new CsvCatalogReader());

try
{
    await seeder.SeedIfEmptyAsync(db, seedDirectory);
}
catch (SpreadsheetFormatException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("The database was not populated. Fix the spreadsheet and restart.");
    return;
}

var products = await db.Products
    .AsNoTracking()
    .Include(p => p.Category)
    .OrderBy(p => p.Name)
    .ToListAsync();

var sales = await db.Sales
    .AsNoTracking()
    .Include(s => s.SaleItems)
    .OrderBy(s => s.SoldAtUtc)
    .ToListAsync();

Console.WriteLine($"Loaded {products.Count} product(s) and {sales.Count} sale(s).");
foreach (var p in products)
    Console.WriteLine($"  {p.Name} — R {p.Price:0.00} — stock {p.StockQuantity} — {p.Category?.Name}");

var writer = new PdfReportWriter();
while (true)
{
    Console.WriteLine();
    Console.WriteLine("1) Export PDF report");
    Console.WriteLine("2) List sales");
    Console.WriteLine("q) Quit");
    var choice = Console.ReadLine()?.Trim();

    if (choice is "q" or "Q") break;

    switch (choice)
    {
        case "1":
            var output = $"report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
            try
            {
                writer.Write(products, sales, output);
                Console.WriteLine($"Report written to {Path.GetFullPath(output)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write report: {ex.Message}");
            }
            break;

        case "2":
            foreach (var s in sales)
                Console.WriteLine($"  {s.CustomerName} — {s.SoldAtUtc:yyyy-MM-dd} — {s.SaleItems.Count} item(s) — R {s.Total:0.00}");
            break;

        default:
            Console.WriteLine("Unknown option.");
            break;
    }
}

static string ResolveSeedPath(string[] args)
{
    var seedIndex = Array.IndexOf(args, "--seed");
    return seedIndex >= 0 && seedIndex + 1 < args.Length
        ? args[seedIndex + 1]
        : Path.Combine(AppContext.BaseDirectory, "Data", "seed");
}
