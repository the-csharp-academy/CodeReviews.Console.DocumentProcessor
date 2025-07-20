using Excel_Reader.Data;
using Excel_Reader.Models;

namespace Excel_Reader.Repository;

public class ExcelReaderRepository<T> : IExcelReaderRepository<Finances>
{
    private readonly ExcelReaderDbContext _context;

    public ExcelReaderRepository(ExcelReaderDbContext context)
    {
        _context = context;
    }

    public void AddFinances(Finances financeData)
    {
        _context.FinancesList.Add(financeData);

        _context.SaveChanges();
    }

    public List<Finances> GetAllData()
    {
        return _context.FinancesList.ToList();
    }
}
