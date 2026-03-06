namespace kilozdazolik.DocumentProcessor.Services.Interfaces
{
    public interface IImportService
    {
        Task<ImportResult> ImportFromCsvAsync(string path);
    }
}
