namespace BusinessObjects.DTOs.Reservations;

public class ReservationQueryParams
{
    public string? Status { get; set; }
    public int? TableId { get; set; }
    public int? StaffId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
