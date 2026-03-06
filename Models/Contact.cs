namespace kilozdazolik.DocumentProcessor.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = null!;

        public int CategoryId { get; set; }

        public Category Category { get; set; }

    }
}
