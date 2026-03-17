using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class DiningTable
{
    public int Id { get; set; }

    public int AreaId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
