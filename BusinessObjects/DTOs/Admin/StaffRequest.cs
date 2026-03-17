using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Admin;

public class StaffRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(15)]
    public string? Phone { get; set; }
}
