using Microsoft.Extensions.Configuration;

namespace ExcelReader.Model;

public class DatabaseUtility
{
    public static string GetConnectionString()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        string projectDirectory = Path.Combine(currentDirectory, @"..\..\..");

        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile($"{projectDirectory}\\app.json", optional: true, reloadOnChange: true)
        .Build();

        string? connectionString = configuration.GetConnectionString("connection");

        return connectionString;
    }
}
