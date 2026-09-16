# Document Processor

A .NET console application that seeds a SQLite database from spreadsheets, displays the
data in the console, and exports it to a PDF report. The seeded data is the **Bean & Barrel
Coffee Roasters** catalogue from the earlier Ecommerce project (the full 4-entity model:
categories, products, sales, and sale items).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (built against 10.0.401).

## Run

```bash
dotnet run --project DocumentProcessor/DocumentProcessor.csproj
```

On first run (empty database) the app seeds itself from the four spreadsheets in
`DocumentProcessor/Data/seed/`. A local `documentprocessor.db` is created next to the
working directory.

The console then lists the products and offers a menu:

| Option | Action |
| --- | --- |
| `1` | Export a PDF report (`report-<timestamp>.pdf`) to the working directory |
| `2` | List sales (customer, date, item count, total) |
| `q` | Quit |

### Seed from a different location

Pass the `--seed <directory>` flag to point the seeder at a folder containing
`categories.csv`, `products.csv`, `sales.csv`, and `sale_items.csv`:

```bash
dotnet run --project DocumentProcessor/DocumentProcessor.csproj -- --seed "C:\path\to\seed"
```

## How it works

- **Seeding** (`Data/SpreadsheetDbSeeder.cs`): runs only when the database is empty
  (`if no data → populate`). It reads the four CSV files, resolves the relational graph by
  name (category name → `Category`, product name → `Product`, customer + timestamp →
  `Sale`), and snapshots each sale line's unit price and product name.
- **Reading** (`Services/CsvCatalogReader.cs`): parses the four CSVs with
  [CsvHelper](https://joshclose.github.io/CsvHelper/) into flat seed DTOs, independent of
  the EF entities.
- **Reporting** (`Services/PdfReportWriter.cs`): builds a PDF with
  [QuestPDF](https://www.questpdf.com/) — a product catalogue table and a sales summary
  table.
- **Error handling** (`Services/SpreadsheetFormatException.cs`): a missing file, wrong
  path, malformed row, or a reference to an unknown product/sale/category throws one typed
  exception, caught in `Program.cs` so the app prints a message and exits cleanly instead
  of crashing.

## Requirement traceability

| Requirement | Where |
| --- | --- |
| Seed the database from a spreadsheet on first run (no data → populate) | `Data/SpreadsheetDbSeeder.cs`, `Data/seed/*.csv` |
| Support `.xls` / `.xlsx` / `.csv` (one is sufficient) | CSV via `Services/CsvCatalogReader.cs` |
| Fetch data from the database and show it in the console | `Program.cs` (product list on startup, sales via option `2`) |
| Report functionality that exports to a PDF file | `Services/PdfReportWriter.cs`, option `1` |
| Handle corrupted files / wrong paths without crashing | `Services/SpreadsheetFormatException.cs`, `try/catch` in `Program.cs` |

## Tests

```bash
dotnet test DocumentProcessor.Tests/DocumentProcessor.Tests.csproj
```

8 tests cover the CSV reader (valid input, missing file, malformed row), the seeder
(empty-DB seeding, idempotency, broken reference), and the PDF writer (non-empty output).

## Notes

- QuestPDF is used under its **Community** license (`LicenseType.Community`), free for
  individuals and small organisations (under $1M annual revenue). See
  <https://www.questpdf.com/license>.
- The legacy `.xls` binary format is not supported; `.csv` is used for seeding, and
  `.xlsx` could be added via the `ICatalogReader` interface.
