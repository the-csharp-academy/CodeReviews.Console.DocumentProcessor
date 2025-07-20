using Excel_Reader.Controller;
using Excel_Reader.Data;
using Excel_Reader.Repository;
using Excel_Reader.Services;
using Excel_Reader.Views;
using Microsoft.Extensions.DependencyInjection;

// Configure the DI container and register dependencies
var services = new ServiceCollection();

services.AddDbContext<ExcelReaderDbContext>();
services.AddScoped(typeof(IExcelReaderRepository<>), typeof(ExcelReaderRepository<>));
services.AddScoped<IExcelReaderService, ExcelReaderService>();
services.AddScoped<ExcelReaderController>();
services.AddScoped<Menu>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

// Set up the database
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExcelReaderDbContext>();

    dbContext.Database.EnsureDeleted();
    Console.WriteLine("Deleting database...");
    dbContext.Database.EnsureCreated();
    Console.WriteLine("Creating database...");
}

// Run the application
var menu = serviceProvider.GetRequiredService<Menu>();
menu.MainMenu();

// Dispose of the service provider
serviceProvider.Dispose();
