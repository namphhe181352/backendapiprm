using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Invoices;

public class CheckoutRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = string.Empty;

    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TipAmount { get; set; }
}
