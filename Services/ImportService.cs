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
    public record ImportResult(int ImportedCount, List<string> Errors);

    public class ImportService(PhoneBookContext context, ILogger<ImportService> logger) : IImportService
    {
        public async Task<ImportResult> ImportFromCsvAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<Dtos.ContactDto>();
            return await ValidateAndImportAsync(records);
        }

        public async Task<ImportResult> ValidateAndImportAsync(IEnumerable<Dtos.ContactDto> dtos)
        {
            var categories = await context.Categories.ToDictionaryAsync(c => c.Name.ToLower(), c => c);
            var existingEmails = new HashSet<string>(
                await context.Contacts.Select(c => c.Email).ToListAsync(),
                StringComparer.OrdinalIgnoreCase
            );

            var errors = new List<string>();
            int importedCount = 0;

            foreach (var dto in dtos)
            {
                var error = Validate(dto, categories, existingEmails);
                if (error is not null)
                {
                    logger.LogWarning(error);
                    errors.Add(error);
                    continue;
                }

                var contact = MapToContact(dto, categories[dto.CategoryName.ToLower()]);

                if (!string.IsNullOrWhiteSpace(dto.Email)) 
                {
                    existingEmails.Add(dto.Email);
                }

                context.Contacts.Add(contact);
                importedCount++;
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Import completed. {ImportedCount} records imported.", importedCount);
            return new ImportResult(importedCount, errors);
        }

        private static string? Validate(Dtos.ContactDto dto, Dictionary<string, Category> categories, HashSet<string> existingEmails)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return $"SKIPPING: Record with empty Name or PhoneNumber.";

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return $"SKIPPING: Record with empty CategoryName.";

            if (!string.IsNullOrEmpty(dto.Email) && existingEmails.Contains(dto.Email))
                return $"SKIPPING: '{dto.Email}' already exists.";

            if (!categories.ContainsKey(dto.CategoryName.ToLower()))
                return $"SKIPPING: Category '{dto.CategoryName}' not found.";

            return null;
        }

        private static Contact MapToContact(Dtos.ContactDto dto, Category category) => new()
        {
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CategoryId = category.CategoryId
        };
    }
}