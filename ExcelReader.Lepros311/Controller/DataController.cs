using ExcelReader.Model;
using Spectre.Console;

namespace ExcelReader.Controller;

public class DataController
{
    private readonly DataRepository _dataRepository;

    public DataController(DataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public void UpdatePdf(string filePath, string tableName, List<Dictionary<string, object>> data)
    {
        var userInterface = new ExcelReader.View.UserInterface();
        var field = userInterface.PromptForField(data);

        var currentRow = data[0];
        var currentValue = currentRow[field];

        Console.WriteLine($"{field}'s current value: {currentValue}\n");
        var newValue = AnsiConsole.Ask<string>($"{field}'s new value:");

        var updatedKvp = new Dictionary<string, object> { { field, newValue } };

        _dataRepository.UpdatePdfData(filePath, updatedKvp);
        _dataRepository.UpdateDb(updatedKvp, tableName);

        Console.WriteLine("\nPDF updated successfully!\n");
    }
}
