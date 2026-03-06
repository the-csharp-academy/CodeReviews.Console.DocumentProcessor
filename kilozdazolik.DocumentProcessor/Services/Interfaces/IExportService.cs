using System;
using System.Collections.Generic;
using System.Text;

namespace kilozdazolik.DocumentProcessor.Services.Interfaces
{
    public interface IExportService
    {
        Task ExportToCsvAsync(string path);
    }
}
