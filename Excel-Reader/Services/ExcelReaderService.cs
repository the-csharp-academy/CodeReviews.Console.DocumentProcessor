using Excel_Reader.Models;
using Excel_Reader.Repository;
using OfficeOpenXml;

namespace Excel_Reader.Services;

public class ExcelReaderService : IExcelReaderService
{
    private readonly IExcelReaderRepository<Finances> _repository;

    public ExcelReaderService(IExcelReaderRepository<Finances> repository)
    {
        _repository = repository;
        ExcelPackage.License.SetNonCommercialPersonal("Andrew Martinez");
    }

    public void ConvertExcelData()
    {
        var fileName = "Finances.xlsx";
        var excelFilePath = Path.Combine(Environment.CurrentDirectory, fileName);

        using (var excel = new ExcelPackage(excelFilePath))
        {
            var sheet = excel.Workbook.Worksheets[0];

            var rowCount = sheet.Dimension.End.Row;

            for (var row = 2; row <= rowCount; row++)
            {
                var financeData = new Finances
                {
                    Date = DateTime.Parse(sheet.Cells[row, 1].Value.ToString()!),
                    Description = sheet.Cells[row, 2].Value.ToString()!,
                    Category = sheet.Cells[row, 3].Value.ToString()!,
                    Amount = double.Parse(sheet.Cells[row, 4].Value.ToString()!),
                    Type = sheet.Cells[row, 5].Value.ToString()!,
                };

                _repository.AddFinances(financeData);
            }
        }
    }

    public List<Finances> GetAllData()
    {
        return _repository.GetAllData();
    }
}
