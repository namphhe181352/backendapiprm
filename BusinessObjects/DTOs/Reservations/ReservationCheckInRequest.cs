using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs.Reservations;

public class ReservationCheckInRequest : IValidatableObject
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
    public int GuestCount { get; set; } = 1;

    [Required]
    public DateTime CheckInTime { get; set; }

    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckInTime == default)
        {
            yield return new ValidationResult("CheckInTime is required.", [nameof(CheckInTime)]);
        }
    }
}
