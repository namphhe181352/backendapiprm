namespace BusinessObjects.DTOs.Admin;

public class StaffQueryParams
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
