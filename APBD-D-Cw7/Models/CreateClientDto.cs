using System.ComponentModel.DataAnnotations;

namespace APBD_D_Cw7.Models;

public class CreateClientDto
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    public string Telephone { get; set; }

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be 11 digits")]
    public string Pesel { get; set; }
}