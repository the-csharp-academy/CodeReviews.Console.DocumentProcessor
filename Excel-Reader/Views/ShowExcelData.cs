using Excel_Reader.Models;
using Spectre.Console;

namespace Excel_Reader.Views;

public class ShowExcelData
{
    public static void ShowAllExcelData(List<Finances> sheets)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Date");
        table.AddColumn("Description");
        table.AddColumn("Category");
        table.AddColumn("Amount");
        table.AddColumn("Type");

        foreach (var sheet in sheets)
            table.AddRow(
                sheet.Id.ToString(),
                sheet.Date.ToString("g"),
                sheet.Description,
                sheet.Category,
                sheet.Amount.ToString("$0.##"),
                sheet.Type
            );

        table.Expand();
        AnsiConsole.Write(table);
    }
}
