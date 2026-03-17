using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Area
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<DiningTable> DiningTables { get; set; } = new List<DiningTable>();
}
