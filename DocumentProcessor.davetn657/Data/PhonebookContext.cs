using DocumentProcessor.davetn657.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DocumentProcessor.davetn657.Data;

public class PhonebookContext : DbContext
{
    public DbSet<PhonebookProperties> Contacts { get; set; }
    private string DbPath { get; set; }

    public PhonebookContext()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        DbPath = builder.GetConnectionString("DefaultConnection");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(DbPath);
}