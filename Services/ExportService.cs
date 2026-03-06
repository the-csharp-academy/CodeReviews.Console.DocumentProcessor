using CsvHelper;
using CsvHelper.Configuration;
using kilozdazolik.DocumentProcessor.Data;
using kilozdazolik.DocumentProcessor.Models;
using kilozdazolik.DocumentProcessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace kilozdazolik.DocumentProcessor.Services
{
    public class ExportService(PhoneBookContext context, ILogger<ExportService> logger) : IExportService
    {
        public async Task ExportToCsvAsync(string filePath)
        {
            var contacts = await FetchContactsAsync();
            await WriteCsvAsync(filePath, contacts);
            logger.LogInformation("Export completed. {Count} records exported.", contacts.Count);
        }

        private async Task<List<Dtos.ContactDto>> FetchContactsAsync()
        {
            return await context.Contacts
                .Include(c => c.Category)
                .Select(c => new Dtos.ContactDto(c.Name, c.Email, c.PhoneNumber, c.Category.Name))
                .ToListAsync();
        }

        private static async Task WriteCsvAsync(string filePath, List<Dtos.ContactDto> contacts)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
            await using var writer = new StreamWriter(filePath);
            await using var csv = new CsvWriter(writer, config);
            await csv.WriteRecordsAsync(contacts);
        }
    }
}