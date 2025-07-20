using Excel_Reader.Controller;
using Spectre.Console;

namespace Excel_Reader.Views;

public class Menu
{
    private readonly ExcelReaderController _controller;

    public Menu(ExcelReaderController controller)
    {
        _controller = controller;
    }

    public void MainMenu()
    {
        AnsiConsole.WriteLine("Reading excel file...");
        _controller.ConvertExcelData();
        AnsiConsole.WriteLine("Creating table...");
        Thread.Sleep(1500);
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Excel Reader"));
        _controller.ReadExcelData();
        AnsiConsole.MarkupLine("[blue]Press any key to exit...[/]");
        Console.ReadKey(true);
    }
}
