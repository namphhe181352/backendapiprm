using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Settings;

public class UpdateMySettingsRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}
