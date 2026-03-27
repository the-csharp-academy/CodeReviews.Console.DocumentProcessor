using DocumentProcessor.davetn657.Data;
using DocumentProcessor.davetn657.Services;
using DocumentProcessor.davetn657.Views;
using IronSoftware.Abstractions.Pdf;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.davetn657;

internal class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddDbContext<PhonebookContext>()
            .AddScoped<IFileReaderService, FileReaderService>()
            .AddScoped<IExtensibleRenderer, ChromePdfRenderer>()
            .AddScoped<IExportDataService, ExportDataService>()
            .AddScoped<IDataSeederService, DataSeederService>()
            .AddScoped<UserInterface>()
            .BuildServiceProvider();

        using var scope = services.CreateScope();

        var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
        dataSeeder.SeedIfEmpty();

        var ui = scope.ServiceProvider.GetRequiredService<UserInterface>();
        ui.Start();
    }
}