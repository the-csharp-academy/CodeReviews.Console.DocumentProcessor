using kilozdazolik.DocumentProcessor.Models;
using kilozdazolik.DocumentProcessor.Services;
using kilozdazolik.DocumentProcessor.Services.Interfaces;
using Spectre.Console;

namespace kilozdazolik.DocumentProcessor
{
    public class UI(IImportService importService,
    IExportService exportService,
    IReportService reportService,
    ReportScheduler scheduler)
    {
        public async Task RunAsync()
        {
            AnsiConsole.Write(
                new FigletText("PhoneBook")
                    .Centered());

            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[grey]What would you like to do?[/]")
                        .AddChoices("Import from CSV", "Export to CSV", "Generate PDF Report", "Exit"));

                switch (choice)
                {
                    case "Import from CSV":
                        await HandleImportAsync();
                        break;
                    case "Export to CSV":
                        await HandleExportAsync();
                        break;
                    case "Generate PDF Report":
                        await HandleReportAsync();
                        break;
                    case "Exit":
                        AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
                        return;
                }
            }
        }

        private async Task HandleImportAsync()
        {
            var filePath = AnsiConsole.Ask<string>("Enter [aqua]CSV file path[/] to import:");

            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found:[/] {filePath}");
                return;
            }

            try
            {
                ImportResult? result = null;
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync("Importing...", async _ =>
                    {
                        result = await importService.ImportFromCsvAsync(filePath);
                    });

                AnsiConsole.MarkupLine($"[green]Done![/] Imported [aqua]{result!.ImportedCount}[/] records.");

                if (result.Errors.Count > 0)
                {
                    var table = new Table()
                        .Border(TableBorder.Rounded)
                        .BorderColor(Spectre.Console.Color.Red)
                        .AddColumn("[red]Skipped / Errors[/]");

                    foreach (var error in result.Errors)
                        table.AddRow(Markup.Escape(error));

                    AnsiConsole.Write(table);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Import failed:[/] {Markup.Escape(ex.Message)}");
            }
        }

        private async Task HandleExportAsync()
        {
            var filePath = AnsiConsole.Ask<string>("Enter [aqua]output file path[/]:", "contacts.csv");

            try
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync("Exporting...", async _ =>
                    {
                        await exportService.ExportToCsvAsync(filePath);
                    });

                AnsiConsole.MarkupLine($"[green]Done![/] Exported to [aqua]{filePath}[/].");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Export failed:[/] {Markup.Escape(ex.Message)}");
            }
        }

        private async Task HandleReportAsync()
        {
            var mode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Report Mode")
                    .AddChoices("Generate Once", "Setup Auto-Generator", "Disable Auto-Generator"));

            if (mode == "Generate Once")
            {
                var filePath = AnsiConsole.Ask<string>("Enter [aqua]output PDF[/]:", "phonebook_report.pdf");

                try
                {
                    await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Dots)
                        .StartAsync("Generating report...", async _ =>
                        {
                            await reportService.ExportToPdfAsync(filePath);
                        });

                    AnsiConsole.MarkupLine($"[green]Done![/] Report saved to [aqua]{filePath}[/].");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Report failed:[/] {Markup.Escape(ex.Message)}");
                }
            }
            else if (mode == "Setup Auto-Generator")
            {
                var minutes = AnsiConsole.Ask<int>("Enter interval in [aqua]minutes[/]:", 10);
                var path = AnsiConsole.Ask<string>("Enter [aqua]filename[/] for auto-reports:", "auto_report.pdf");

                // 3. Use 'scheduler' (the name from your constructor)
                scheduler.IntervalMinutes = minutes;
                scheduler.OutputPath = path;
                scheduler.IsEnabled = true;

                AnsiConsole.MarkupLine($"[green]Auto-generator enabled![/] Running every {minutes} minutes.");
            }
            else
            {
                scheduler.IsEnabled = false;
                AnsiConsole.MarkupLine("[yellow]Auto-generator disabled.[/]");
            }
        }
    }
}