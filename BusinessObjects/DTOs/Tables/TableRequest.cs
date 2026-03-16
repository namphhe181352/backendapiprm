using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Tables;

public class TableRequest
{
    [Required]
    public int AreaId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Capacity { get; set; } = 4;

    [MaxLength(20)]
    public string Status { get; set; } = "available";
}
