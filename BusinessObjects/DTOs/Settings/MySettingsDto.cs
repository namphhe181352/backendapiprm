namespace BusinessObjects.DTOs.Settings;

public class MySettingsDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
