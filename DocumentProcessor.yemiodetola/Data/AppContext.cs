using Microsoft.EntityFrameworkCore;
using DocumentProcessor.yemiodetola.Models;
public class AppDbContext : DbContext
{
  public DbSet<Contact> Contacts => Set<Contact>();

  protected override void OnConfiguring(DbContextOptionsBuilder options)
  {
    var dbName = System.AppContext.GetData("SqliteDbName") as string ?? "excel_data.db";
    options.UseSqlite($"Data Source={dbName}");
  }
}