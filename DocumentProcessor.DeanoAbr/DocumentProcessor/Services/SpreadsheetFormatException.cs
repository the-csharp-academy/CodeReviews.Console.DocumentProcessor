namespace DocumentProcessor.Services;

/// <summary>
/// Thrown when seed data cannot be read or is inconsistent — a missing file, a wrong path,
/// a malformed row, or a reference to a product/sale/category that does not exist.
/// </summary>
public sealed class SpreadsheetFormatException : Exception
{
    public SpreadsheetFormatException(string message) : base(message) { }
    public SpreadsheetFormatException(string message, Exception inner) : base(message, inner) { }
}
