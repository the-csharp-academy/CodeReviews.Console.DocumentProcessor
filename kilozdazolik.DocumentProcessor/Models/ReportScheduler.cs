namespace kilozdazolik.DocumentProcessor.Models
{
    public class ReportScheduler
    {
        public bool IsEnabled { get; set; } = false;
        public int IntervalMinutes { get; set; } = 10;
        public string OutputPath { get; set; } = "AutomaticReport.pdf";
    }
}
