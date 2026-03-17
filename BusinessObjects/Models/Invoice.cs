using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int StaffId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TipAmount { get; set; }

    public decimal FinalTotal { get; set; }

    public DateTime PaidAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
