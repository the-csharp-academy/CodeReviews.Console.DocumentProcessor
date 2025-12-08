using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentProcessor.yemiodetola.Models;
namespace DocumentProcessor.yemiodetola.Services;
public class ExcelService : IExcelService
{
    public async Task<List<Contact>> ReadExcelAsync(string path)
    {
        // For demo, call ExcelReader synchronously
        return await Task.Run(() => ExcelReader.ListContacts(path, "Contacts"));
    }
}
