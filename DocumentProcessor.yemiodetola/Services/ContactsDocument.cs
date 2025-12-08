using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using DocumentProcessor.yemiodetola.Models;

namespace DocumentProcessor.yemiodetola.Services;

public class ContactsDocument : IDocument
{
  private readonly List<Contact> _contacts;
  public ContactsDocument(List<Contact> contacts)
  {
    _contacts = contacts;
  }

  public DocumentMetadata GetMetadata() => new DocumentMetadata();
  public DocumentSettings GetSettings() => new DocumentSettings();

  public void Compose(IDocumentContainer container)
  {
    container.Page(page =>
    {
      page.Margin(30);

      page.Header().Text("Contact List")
              .FontSize(20)
              .SemiBold()
              .AlignCenter();

      page.Content().Table(table =>
          {
            table.ColumnsDefinition(columns =>
                {
                  columns.ConstantColumn(150);
                  columns.RelativeColumn();
                  columns.ConstantColumn(120);
                });
            table.Header(header =>
                {
                  header.Cell().Text("Name").SemiBold();
                  header.Cell().Text("Email").SemiBold();
                  header.Cell().Text("Phone Number").SemiBold();
                });
            foreach (var c in _contacts)
            {
              table.Cell().Text(c.Name);
              table.Cell().Text(c.Email);
              table.Cell().Text(c.PhoneNumber);
            }
          });
    });
  }
}

