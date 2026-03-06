using kilozdazolik.DocumentProcessor;
using kilozdazolik.DocumentProcessor.Data;
using kilozdazolik.DocumentProcessor.Models;
using kilozdazolik.DocumentProcessor.Services;
using kilozdazolik.DocumentProcessor.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<PhoneBookContext>(); 
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddSingleton<UI>();
builder.Services.AddSingleton<ReportScheduler>();
builder.Services.AddHostedService<WorkerService>();

using IHost host = builder.Build();

await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    var ui = scope.ServiceProvider.GetRequiredService<UI>();
    await ui.RunAsync();
}

await host.StopAsync();