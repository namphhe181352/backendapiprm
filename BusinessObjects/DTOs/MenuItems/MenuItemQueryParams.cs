namespace BusinessObjects.DTOs.MenuItems;

public class MenuItemQueryParams
{
    public int? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
