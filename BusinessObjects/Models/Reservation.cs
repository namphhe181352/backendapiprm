using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public int StaffId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public int GuestCount { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User Staff { get; set; } = null!;

    public virtual DiningTable Table { get; set; } = null!;
}
