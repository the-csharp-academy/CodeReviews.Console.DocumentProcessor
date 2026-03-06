using Moq;
using Microsoft.EntityFrameworkCore;
using kilozdazolik.DocumentProcessor.Services;
using kilozdazolik.DocumentProcessor.Data;
using kilozdazolik.DocumentProcessor.Models;
using Microsoft.Extensions.Logging;

public class ImportServiceTests
{
    private PhoneBookContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PhoneBookContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new PhoneBookContext(options);
    }

    [Fact]
    public async Task ValidateAndMap_ShouldSkipRecord_WhenNameIsEmpty()
    {
        // Arrange
        var context = GetInMemoryContext();

        var category = new Category { Name = "Work" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ImportService>>();
        var service = new ImportService(context, loggerMock.Object);

        var dtos = new List<Dtos.ContactDto>
        {
            new Dtos.ContactDto("", "test@test.com", "123456", "Work")
        };

        // Act
        await service.ValidateAndImportAsync(dtos);

        // Assert
        var count = await context.Contacts.CountAsync();
        Assert.Equal(0, count); 
    }

    [Fact]
    public async Task ValidateAndImportAsync_ShouldAllowEmptyEmail_WhenNameAndPhoneAreValid()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PhoneBookContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        using var context = new PhoneBookContext(options);

        var category = new Category { Name = "Work" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ImportService>>();
        var service = new ImportService(context, loggerMock.Object);

        var dtos = new List<Dtos.ContactDto>
    {
        new Dtos.ContactDto("John Doe", "", "123456", "Work")
    };

        // Act
        await service.ValidateAndImportAsync(dtos);

        // Assert 
        var contact = await context.Contacts
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Name == "John Doe");

        Assert.NotNull(contact);
        Assert.Equal("123456", contact.PhoneNumber);
        Assert.Equal("", contact.Email);
    }
}