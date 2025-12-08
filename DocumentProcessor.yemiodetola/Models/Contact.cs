namespace DocumentProcessor.yemiodetola.Models;

public class Contact
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;

  public bool validateEmailAddress(string email)
  {
    if (String.IsNullOrEmpty(email))
    {
      return false;
    }
    int atIndex = email.IndexOf('@');
    int dotIndex = email.LastIndexOf('.');

    return atIndex > 0
      && dotIndex > atIndex + 1
      && dotIndex < email.Length - 1;
  }

  public bool validatePhoneNumber(string phoneNumber)
  {
    return !String.IsNullOrEmpty(phoneNumber)
      && phoneNumber.Length == 11
      && phoneNumber.StartsWith("0")
      && phoneNumber.All(char.IsDigit);
  }
}