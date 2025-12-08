using ClosedXML.Excel;
using DocumentProcessor.yemiodetola.Models;

namespace DocumentProcessor.yemiodetola.Services;
public static class ExcelReader
{
  public static List<Contact> ListContacts(string path, string sheet)
  {
    var results = new List<Contact>();
    using var workbook = new XLWorkbook(path);
    var ws = workbook.Worksheet(sheet);

    foreach (var row in ws.RowsUsed().Skip(1))
    {
      var name = row.Cell(1).GetString().Trim();
      var email = row.Cell(2).GetString().Trim();
      var phone = row.Cell(3).GetString().Trim();

      var model = new Contact
      {
        Name = name,
        Email = email,
        PhoneNumber = phone
      };

      if (!string.IsNullOrWhiteSpace(model.Name))
      {
        results.Add(model);
      }
    }
    return results;
  }
}