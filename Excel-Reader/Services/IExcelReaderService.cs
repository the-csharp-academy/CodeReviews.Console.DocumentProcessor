using Excel_Reader.Models;

namespace Excel_Reader.Services;

public interface IExcelReaderService
{
    public void ConvertExcelData();

    public List<Finances> GetAllData();
}
