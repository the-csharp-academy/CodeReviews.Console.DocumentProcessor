namespace Excel_Reader.Repository;

public interface IExcelReaderRepository<T>
{
    void AddFinances(T financeData);

    List<T> GetAllData();
}
