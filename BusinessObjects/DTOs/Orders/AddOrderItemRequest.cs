using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Orders;

public class AddOrderItemRequest
{
    [Required]
    public int MenuItemId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    public string? Note { get; set; }
}
