namespace BusinessObjects.DTOs.Admin;

public class RevenueStatisticsDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TotalInvoices { get; set; }
}
