using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Orders;

public class UpdateOrderItemStatusRequest
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
