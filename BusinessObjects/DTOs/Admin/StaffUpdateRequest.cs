using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Admin;

public class StaffUpdateRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(15)]
    public string? Phone { get; set; }

    [MinLength(6)]
    public string? Password { get; set; }
}
