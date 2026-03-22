namespace BusinessObjects.DTOs.Admin;

public class StatisticsOverviewDto
{
    public RevenueStatisticsDto Revenue { get; set; } = new();
    public List<TopItemDto> TopItems { get; set; } = [];
}
