using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Tables;

public class ChangeTableStatusRequest
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
