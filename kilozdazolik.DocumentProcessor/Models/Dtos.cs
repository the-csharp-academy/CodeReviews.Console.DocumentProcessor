using System;
using System.Collections.Generic;
using System.Text;

namespace kilozdazolik.DocumentProcessor.Models
{
    public class Dtos
    {
        public record ContactDto(
            string Name,
            string Email,
            string PhoneNumber,
            string CategoryName
        );
    }
}
