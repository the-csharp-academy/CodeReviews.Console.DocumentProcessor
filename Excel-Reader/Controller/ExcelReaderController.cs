using Excel_Reader.Services;
using Excel_Reader.Views;

namespace Excel_Reader.Controller;

public class ExcelReaderController
{
    private readonly IExcelReaderService _service;

    public ExcelReaderController(IExcelReaderService service)
    {
        _service = service;
    }

    public void ConvertExcelData()
    {
        _service.ConvertExcelData();
    }

    public void ReadExcelData()
    {
        var sheets = _service.GetAllData();

        ShowExcelData.ShowAllExcelData(sheets);
    }
}
