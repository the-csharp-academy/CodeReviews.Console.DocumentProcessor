using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentProcessor.Models;

/// <summary>
/// Join row between Sale and Product. It carries a receipt snapshot, so sales history never
/// changes when a product is edited or retired.
/// </summary>
public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }

    /// <summary>Snapshot of Product.Price at the moment of the sale.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Snapshot of Product.Name at the moment of the sale.</summary>
    public string ProductName { get; set; } = string.Empty;

    [NotMapped]
    public decimal LineTotal => Quantity * UnitPrice;
}
