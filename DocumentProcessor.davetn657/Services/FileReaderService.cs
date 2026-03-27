using DocumentProcessor.davetn657.Data.Models;
using IronXL;
using Spectre.Console;

namespace DocumentProcessor.davetn657.Services;

public interface IFileReaderService
{
    public List<PhonebookProperties> FormatFile(string filePath, string fileName);
}

public class FileReaderService : IFileReaderService
{
    public FileReaderService()
    {

    }

    public List<PhonebookProperties> FormatFile(string filePath, string fileName)
    {
        var fullPath = Path.Combine(filePath, fileName);
        var file = fileName.Split('.');
        var fileType = file[1];

        var properties = new List<PhonebookProperties>();
        WorkSheet? workSheet;

        switch (fileType)
        {
            case "xlsx":
                workSheet = ReadXlsxFile(fullPath);
                break;
            case "csv":
                workSheet = ReadCsvFile(fullPath);
                break;
            default:
                workSheet = null;
                break;
        }

        if (workSheet == null)
        {
            AnsiConsole.WriteLine("File is empty or could not open!");
            AnsiConsole.Prompt(new TextPrompt<string>("Return?").AllowEmpty());
            return [];
        }
        properties = FormatWorkSheetData(workSheet);

        return properties;
    }

    private WorkSheet? ReadXlsxFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File doesn't exist");
            return null;
        }

        var workBook = WorkBook.Load(filePath);
        var workSheet = workBook.WorkSheets.First();

        return workSheet;
    }

    private WorkSheet? ReadCsvFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File doesn't exist");
            return null;
        }

        var workBook = WorkBook.LoadCSV(filePath, ExcelFileFormat.XLSX, listDelimiter: ",");
        var workSheet = workBook.DefaultWorkSheet;

        return workSheet;
    }

    private List<PhonebookProperties> FormatWorkSheetData(WorkSheet sheetData)
    {
        var properties = new List<PhonebookProperties>();
        var rowCount = sheetData.RowCount;

        for (var i = 2; i < rowCount; i++)
        {
            try
            {
                var data = new PhonebookProperties
                {
                    Name = sheetData["A" + i].TryGetValue<string>(out var name) ? name : string.Empty,
                    PhoneNumber = sheetData["B" + i].TryGetValue<string>(out var phone) ? phone : string.Empty,
                    Email = sheetData["C" + i].TryGetValue<string>(out var email) ? email : string.Empty,
                    Category = sheetData["D" + i].TryGetValue<string>(out var category) ? category : string.Empty,
                };

                properties.Add(data);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine("Could not create property! spreadsheet format is not correct!");
                AnsiConsole.WriteLine(ex.Message);
                AnsiConsole.Prompt(new TextPrompt<string>("Return").AllowEmpty());
                break;
            }
        }

        return properties;
    }
}