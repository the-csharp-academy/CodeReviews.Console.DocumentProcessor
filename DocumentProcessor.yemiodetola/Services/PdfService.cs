using System.Collections.Generic;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using DocumentProcessor.yemiodetola.Models;
namespace DocumentProcessor.yemiodetola.Services;
public class PdfService : IPdfService
{
    public async Task GeneratePdfAsync(string outputPath, List<Contact> contacts)
    {
        await Task.Run(() =>
        {
            var document = new ContactsDocument(contacts);
            document.GeneratePdf(outputPath);
        });
    }
}
