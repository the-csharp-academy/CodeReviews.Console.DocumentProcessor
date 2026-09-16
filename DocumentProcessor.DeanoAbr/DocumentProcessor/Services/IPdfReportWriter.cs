using DocumentProcessor.Models;

namespace DocumentProcessor.Services;

public interface IPdfReportWriter
{
    /// <summary>Writes a PDF report of the products and sales to the given path.</summary>
    void Write(IReadOnlyList<Product> products, IReadOnlyList<Sale> sales, string outputPath);
}
