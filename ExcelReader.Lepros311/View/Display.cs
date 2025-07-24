using ExcelReader.Model;
using Spectre.Console;
using System.Diagnostics;

namespace ExcelReader.View;

public class Display
{
    public static void PrintAllData(string tableName, bool hasIdColumn)
    {
        var repository = new DataRepository(DatabaseUtility.GetConnectionString());
        var data = repository.GetAllData(tableName);

        var rule = new Rule($"[green]{tableName}[/]");
        rule.Justification = Justify.Left;
        AnsiConsole.Write(rule);

        if (data == null || data.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No records found.[/]");
            return;
        }

        var table = new Table();

        var columnWidths = new Dictionary<string, int>();

        foreach (var key in data[0].Keys)
        {
            string header = key;
            int maxColWidth = header.Length + 10;

            foreach (var row in data)
            {
                string value = row[key]?.ToString() ?? string.Empty;
                maxColWidth = Math.Max(maxColWidth, value.Length + 10);
            }

            if (key == "Id" && hasIdColumn)
            {
                table.AddColumn(new TableColumn($"[dodgerblue1]{key}[/]").Centered().NoWrap());
                columnWidths[key] = maxColWidth;
            }
            else if (key != "Id")
            {
                table.AddColumn(new TableColumn($"[dodgerblue1]{key}[/]").Centered().NoWrap());
                columnWidths[key] = maxColWidth;
            }
        }

        foreach (var row in data)
        {
            var rowValues = row.Values.Select(value => $"  {value?.ToString() ?? string.Empty}  ").ToArray();

            if (hasIdColumn)
            {
                table.AddRow(rowValues);
            }
            else
            {
                var filteredRowValues = rowValues.Where((_, index) => data[0].Keys.ElementAt(index) != "Id").ToArray();
                table.AddRow(filteredRowValues);
            }
        }

        Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

        AnsiConsole.Write(table.ShowRowSeparators().Border(TableBorder.DoubleEdge));
    }

    public static void OpenFileInExternalProgram(string filePath)
    {
        Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
    }
}
