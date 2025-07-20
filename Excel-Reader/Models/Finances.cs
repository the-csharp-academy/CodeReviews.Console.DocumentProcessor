using System.ComponentModel.DataAnnotations;

namespace Excel_Reader.Models;

public class Finances
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public double Amount { get; set; }

    public string Type { get; set; } = string.Empty;
}
