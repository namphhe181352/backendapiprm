using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Categories;

public class CategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }
}
