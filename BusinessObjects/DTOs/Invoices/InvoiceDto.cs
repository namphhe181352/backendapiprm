namespace BusinessObjects.DTOs.Invoices;

public class InvoiceDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int StaffId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TipAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public DateTime PaidAt { get; set; }
}
