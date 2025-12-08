using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentProcessor.yemiodetola.Models;
namespace DocumentProcessor.yemiodetola.Services;
public interface IExcelService
{
    Task<List<Contact>> ReadExcelAsync(string path);
}
