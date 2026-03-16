using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Areas;

public class AreaRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}
