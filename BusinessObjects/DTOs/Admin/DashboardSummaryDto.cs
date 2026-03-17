namespace BusinessObjects.DTOs.Admin;

public class DashboardSummaryDto
{
    public int TotalTables { get; set; }
    public int OccupiedTables { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
}
