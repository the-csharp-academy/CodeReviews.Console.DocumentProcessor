using DocumentProcessor.davetn657.Data;
using DocumentProcessor.davetn657.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace DocumentProcessor.davetn657.Views;

public class UserInterface
{
    private readonly IFileReaderService _fileReader;
    private readonly IExportDataService _exporter;
    private readonly PhonebookContext _dbContext;

    public UserInterface(IFileReaderService fileReader, IExportDataService exporter, PhonebookContext dbContext)
    {
        _fileReader = fileReader;
        _exporter = exporter;
        _dbContext = dbContext;
    }

    public void Start()
    {
        while (true)
        {
            TitleCard("Main Menu");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "DocFiles");
            try
            {
                var fileNames = Directory.GetFiles(filePath)
                    .Where(s => s.EndsWith(".xlsx") || s.EndsWith(".csv"));

                var menuOptions = new List<string>()
                {
                    "Exit",
                    "Export",
                    "Delete all database data"
                };

                foreach (var file in fileNames)
                {
                    menuOptions.Add(Path.GetFileName(file));
                }

                AnsiConsole.WriteLine("All files in DocFiles directory");
                var selected = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(menuOptions));

                switch (selected)
                {
                    case "Exit":
                        return;
                    case "Export":
                        ExportData();
                        break;
                    case "Delete all database data":
                        _dbContext.Contacts.ExecuteDelete();
                        _dbContext.SaveChanges();
                        break;
                    default:
                        FileDetails(selected, filePath);
                        break;
                }
            }
            catch (DirectoryNotFoundException ex)
            {

                AnsiConsole.WriteLine($"Couldn't find file path");
                AnsiConsole.WriteLine("Error: " + ex);
                AnsiConsole.WriteLine("Try entering a new path?");

                var selected = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(new string[] { "Yes", "No" }));

                if (selected.Equals("No")) return;

                filePath = AnsiConsole.Ask<string>("Enter here:");
            }
        }
    }

    public void FileDetails(string fileName, string filePath)
    {
        TitleCard(fileName + " Details");

        var table = new Table();
        var contacts = _fileReader.FormatFile(filePath, fileName);

        table.AddColumns(new string[]{
            "Name",
            "Phone Number",
            "Email"
        });

        foreach(var contact in contacts.Take(10))
        {
            table.AddRow(contact.Name, contact.PhoneNumber, contact.Email);
        }

        AnsiConsole.WriteLine("Top Excel Rows:");
        AnsiConsole.Write(table);

        var menuOptions = new List<string> 
        {
            "Import Data",
            "Return"
        };

        var selected = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(menuOptions));

        if (selected.Equals("Return")) return;

        _dbContext.Contacts.AddRange(contacts);
        _dbContext.SaveChanges();

        AnsiConsole.WriteLine("Successfully imported data!");
        AnsiConsole.Prompt(new TextPrompt<string>("Return?"));
    }

    public void ExportData()
    {
        TitleCard("Export Current Data");

        var menuOptions = new Dictionary<string, Action?>()
        {
            { "Return", null },
            {".pdf", _exporter.ExportToPdf },
            { ".xlsx", _exporter.ExportToXlsx },
            { ".csv", _exporter.ExportToCsv }
        };

        var selected = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(menuOptions.Keys));

        if (menuOptions.TryGetValue(selected, out var action) && action != null)
        {
            action();
        }

        AnsiConsole.Prompt(new TextPrompt<string>("Return").AllowEmpty());
    }

    public void TitleCard(string title)
    {
        var titleCard = new FigletText(title)
        {
            Justification = Justify.Center,
            Color = Color.Blue1
        };

        AnsiConsole.Clear();
        AnsiConsole.Write(titleCard);
    }
}