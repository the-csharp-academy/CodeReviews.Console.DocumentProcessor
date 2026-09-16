using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentProcessor.Models;

public class Sale
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SoldAtUtc { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    [NotMapped]
    public decimal Total => SaleItems.Sum(i => i.Quantity * i.UnitPrice);
}
