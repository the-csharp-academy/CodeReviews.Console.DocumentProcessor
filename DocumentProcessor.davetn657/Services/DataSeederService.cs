using DocumentProcessor.davetn657.Data;

namespace DocumentProcessor.davetn657.Services;

public interface IDataSeederService
{
    public void SeedIfEmpty();
}

public class DataSeederService : IDataSeederService
{
    private readonly PhonebookContext _dbContext;
    private readonly IFileReaderService _fileReader;
    public DataSeederService(PhonebookContext dbContext, IFileReaderService fileReader)
    {
        _dbContext = dbContext;
        _fileReader = fileReader;
    }

    public void SeedIfEmpty()
    {

        if (!_dbContext.Contacts.Any())
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "DocFiles");
            var contacts = _fileReader.FormatFile(path, "SeedData.xlsx");
            _dbContext.Contacts.AddRange(contacts);
            _dbContext.SaveChanges();
        }
    }
}