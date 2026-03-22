namespace BusinessObjects.DTOs.Admin;

public class RevenueStatisticsDto
{
    public string Period { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal PreviousRevenue { get; set; }
    public decimal RevenueChangePercent { get; set; }
    public int TotalInvoices { get; set; }
    public List<decimal> ChartValues { get; set; } = [];
    public List<string> ChartLabels { get; set; } = [];
}
