using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using QuestPDF.Infrastructure;
using DocumentProcessor.yemiodetola.Services;

class Program
{
  static async Task Main(string[] args)
  {
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    var services = new ServiceCollection();

    services.AddSingleton<DocumentProcessor.yemiodetola.Services.IExcelService, DocumentProcessor.yemiodetola.Services.ExcelService>();
    services.AddSingleton<DocumentProcessor.yemiodetola.Services.IPdfService, DocumentProcessor.yemiodetola.Services.PdfService>();

    var provider = services.BuildServiceProvider();

    var excelService = provider.GetRequiredService<DocumentProcessor.yemiodetola.Services.IExcelService>();
    var pdfService = provider.GetRequiredService<DocumentProcessor.yemiodetola.Services.IPdfService>();

    var excelPath = AppContext.GetData("ExcelFilePath") as string ?? "./sample.xlsx";
    var rows = await excelService.ReadExcelAsync(excelPath);

    if (rows == null || rows.Count == 0)
    {
      Console.WriteLine("Excel file is empty or unreadable.");
      return;
    }

    Console.WriteLine("\nExcel File Contents:\n");
    Console.WriteLine($"{ "Name",-25} { "Email",-35} { "Phone Number",-15}");
    Console.WriteLine(new string('-', 80));
    foreach (var contact in rows)
    {
      Console.WriteLine($"{contact.Name,-25} {contact.Email,-35} {contact.PhoneNumber,-15}");
    }
    Console.WriteLine(new string('-', 80));

    var outputPdf = AppContext.GetData("PdfOutputPath") as string ?? "./output.pdf";

    await pdfService.GeneratePdfAsync(outputPdf, rows);

    Console.WriteLine($"PDF generated successfully at: {outputPdf}");
  }
}
