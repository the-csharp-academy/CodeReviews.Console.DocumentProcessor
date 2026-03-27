using DocumentProcessor.davetn657.Data;
using IronXL;
using IronPdf;
using Spectre.Console;
using System.Text;
using IronSoftware.Abstractions.Pdf;

namespace DocumentProcessor.davetn657.Services;

public interface IExportDataService
{
    void ExportToPdf();
    void ExportToXlsx();
    void ExportToCsv();
}

public class ExportDataService : IExportDataService
{
    private readonly PhonebookContext _dbContext;
    private readonly IExtensibleRenderer _renderer;
    public ExportDataService(PhonebookContext dbContext, IExtensibleRenderer renderer)
    {
        _dbContext = dbContext;
        _renderer = renderer;
    }

    public void ExportToPdf()
    {
        try
        {
            var _renderer = new ChromePdfRenderer();

            var htmlContent = @$"
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    table, th, td 
                    {{
                        border: 1px solid;
                    }}
                </style>
            </head>
            <body>
                <table>
                    <tr>
                        <th>Name</th>
                        <th>Phone Number</th>
                        <th>Email</th>
                        <th>Category</th>
                    </tr>
                    {HtmlTables()}
                </table>
            </body>
            </html>";

            var pdf = _renderer.RenderHtmlAsPdf(htmlContent);

            pdf.SaveAs("DocFiles\\Contacts.pdf");
            AnsiConsole.WriteLine("Successfully exported to pdf");
        }
        catch
        {
            AnsiConsole.WriteLine("Failed to fully export to pdf - data may be missing or incomplete!");
        }
    }

    public void ExportToXlsx()
    {
        ExportWorkBook(wb => wb.SaveAs("DocFiles\\Contacts.xlsx"));
    }

    public void ExportToCsv()
    {
        ExportWorkBook(wb => wb.SaveAsCsv("DocFiles\\Contacts.csv"));
    }

    private string HtmlTables()
    {
        var contacts = _dbContext.Contacts;
        var htmlTableContent = new StringBuilder();

        foreach(var contact in contacts)
        {
            htmlTableContent.Append(@$"
            <tr>
                <td>{contact.Name}</td>
                <td>{contact.PhoneNumber}</td>
                <td>{contact.Email}</td>
                <td>{contact.Category}</td>
            </tr>
            ");
        }

        return htmlTableContent.ToString();
    }

    private void ExportWorkBook(Action<WorkBook> save)
    {
        try
        {
            var contacts = _dbContext.Contacts.ToList();

            var workbook = WorkBook.Create(ExcelFileFormat.XLSX);
            var worksheet = workbook.CreateWorkSheet("Contacts");

            worksheet["A1"].Value = "Names";
            worksheet["B1"].Value = "Phone Numbers";
            worksheet["C1"].Value = "Emails";
            worksheet["D1"].Value = "Categories";

            for (int i = 0; i < contacts.Count; i++)
            {
                var row = i + 2;
                worksheet["A" + row].Value = contacts[i].Name;
                worksheet["B" + row].Value = contacts[i].PhoneNumber;
                worksheet["C" + row].Value = contacts[i].Email;
                worksheet["D" + row].Value = contacts[i].Category;
            }

            save(workbook);
            AnsiConsole.WriteLine("Successfully exported workbook");
        }
        catch
        {
            AnsiConsole.WriteLine("Failed to fully export workbook - some data may be missing or incomplete!");
        }
    }
}