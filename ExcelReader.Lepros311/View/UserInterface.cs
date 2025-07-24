using Spectre.Console;

namespace ExcelReader.View;

public class UserInterface
{
    public string GetFilePath()
    {
        string[] supportedExtensions = { ".xlsx", ".csv", ".pdf" };
        string filePath;

        do
        {
            Console.WriteLine("Please enter the full path of the Excel, CSV, or PDF file (e.g., C:\\path\\to\\your\\file.xlsx [or .csv or .pdf]):");
            filePath = Console.ReadLine();

            if (!supportedExtensions.Contains(Path.GetExtension(filePath).ToLower()))
            {
                Console.WriteLine("Unsupported file type. Please enter a valid Excel, CSV, or PDF file.\n");
                continue;
            }

            Console.WriteLine("\nLooking for the file at: " + filePath + "...");
        }while (!File.Exists(filePath));

        while (!File.Exists(filePath))
        {
            Console.WriteLine("File not found. \nPlease enter a valid file path:");
            filePath = Console.ReadLine();
            Console.WriteLine("\nLooking for the file at: " + filePath + "...");
        }

        Console.WriteLine("File found.\n");

        return filePath;
    }

    public bool PromptForPdfEdit()
    {
        Console.WriteLine();
        bool editPdf = AnsiConsole.Confirm($"Would you like to edit any of the PDF's data?", false);

        return editPdf;
    }

    public string PromptForField(List<Dictionary<string, object>> data)
    {
        var fields = data[0].Keys.Select(key => key.ToString()).ToList();

        var fieldsToDisplay = fields.Skip(1).ToList();

        Console.WriteLine();

        var field = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Choose a field:")
            .AddChoices(fieldsToDisplay));

        return field;
    }

    public bool PromptForExternalOpen(string fileExtension)
    {
        Console.WriteLine();
        bool externalOpen = AnsiConsole.Confirm($"Would you like to open this file in your system's default {fileExtension} program?", false);

        return externalOpen;
    }
}
