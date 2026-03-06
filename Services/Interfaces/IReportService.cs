namespace kilozdazolik.DocumentProcessor.Services.Interfaces
{
    public interface IReportService
    {
        Task ExportToPdfAsync(string filePath);
    }
}