using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentProcessor.yemiodetola.Models;
namespace DocumentProcessor.yemiodetola.Services;
public interface IPdfService
{
    Task GeneratePdfAsync(string outputPath, List<Contact> contacts);
}
