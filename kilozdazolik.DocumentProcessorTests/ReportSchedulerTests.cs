using kilozdazolik.DocumentProcessor.Models;

public class ReportSchedulerTests
{
    [Fact]
    public void Scheduler_ShouldUpdateCorrectly_WhenModified()
    {
        // Arrange
        var scheduler = new ReportScheduler();

        // Act
        scheduler.IsEnabled = true;
        scheduler.IntervalMinutes = 30;
        scheduler.OutputPath = "new_report.pdf";

        // Assert
        Assert.True(scheduler.IsEnabled);
        Assert.Equal(30, scheduler.IntervalMinutes);
        Assert.Equal("new_report.pdf", scheduler.OutputPath);
    }
}