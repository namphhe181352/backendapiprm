namespace BusinessObjects.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? InvoiceId { get; set; }
    public string? InvoicePaymentMethod { get; set; }
    public decimal? InvoiceFinalTotal { get; set; }
    public DateTime? InvoicePaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
