using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Reservations;

public class ReservationRequest
{
    [Required]
    public int TableId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(1, 100)]
    public int GuestCount { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "occupied";

    public string? Note { get; set; }
}
