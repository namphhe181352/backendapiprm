namespace BusinessObjects.DTOs.Admin;

public class TopItemDto
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
