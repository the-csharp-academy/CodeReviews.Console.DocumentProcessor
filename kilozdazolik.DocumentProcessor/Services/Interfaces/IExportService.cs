namespace kilozdazolik.DocumentProcessor.Services.Interfaces
{
    public interface IExportService
    {
        Task ExportToCsvAsync(string path);
    }
}
