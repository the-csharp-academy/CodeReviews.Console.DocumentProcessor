using kilozdazolik.DocumentProcessor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace kilozdazolik.DocumentProcessor.Data;

public class PhoneBookContext : DbContext
{
    public PhoneBookContext() { }
    public PhoneBookContext(DbContextOptions<PhoneBookContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlite(connectionString);
    }
}