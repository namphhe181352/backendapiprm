namespace BusinessObjects.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
