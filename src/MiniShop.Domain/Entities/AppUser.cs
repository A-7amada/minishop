using System.ComponentModel.DataAnnotations;

namespace MiniShop.Domain.Entities;

public class AppUser
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";

    public bool IsAdmin { get; set; }

    public bool IsActive { get; set; } = true;
}
