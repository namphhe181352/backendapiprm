namespace BusinessObjects.DTOs.Invoices;

public class InvoiceQueryParams
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? StaffId { get; set; }
    public string? PaymentMethod { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
