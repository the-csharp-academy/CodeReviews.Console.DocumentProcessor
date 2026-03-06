using kilozdazolik.DocumentProcessor.Models;
using kilozdazolik.DocumentProcessor.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace kilozdazolik.DocumentProcessor.Services
{
    public class WorkerService(
        IServiceScopeFactory scopeFactory,
        ILogger<WorkerService> logger,
        ReportScheduler scheduler) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background Report Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (scheduler.IsEnabled)
                {
                    try
                    {
                        using (var scope = scopeFactory.CreateScope())
                        {
                            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                            var finalPath = scheduler.OutputPath.Replace(".pdf", $"_{DateTime.Now:yyyyMMdd_HHmm}.pdf");

                            await reportService.ExportToPdfAsync(finalPath);
                            logger.LogInformation("Background report generated: {Path}", finalPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to generate background report.");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(scheduler.IntervalMinutes), stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}