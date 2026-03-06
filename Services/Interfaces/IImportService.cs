using kilozdazolik.DocumentProcessor.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace kilozdazolik.DocumentProcessor.Services.Interfaces
{
    public interface IImportService
    {
        Task<ImportResult> ImportFromCsvAsync(string path);
    }
}
