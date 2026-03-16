using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Invoices;
using BusinessObjects.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public Task<Invoice?> GetDetailByIdAsync(int id) =>
        _dbSet
            .Include(x => x.Staff)
            .Include(x => x.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<Invoice>> GetFilteredAsync(InvoiceQueryParams queryParams)
    {
        var query = _dbSet
            .Include(x => x.Staff)
            .Include(x => x.Order)
            .AsQueryable();

        if (queryParams.From.HasValue)
        {
            query = query.Where(x => x.PaidAt >= queryParams.From.Value);
        }

        if (queryParams.To.HasValue)
        {
            query = query.Where(x => x.PaidAt <= queryParams.To.Value);
        }

        if (queryParams.StaffId.HasValue)
        {
            query = query.Where(x => x.StaffId == queryParams.StaffId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.PaymentMethod))
        {
            query = query.Where(x => x.PaymentMethod == queryParams.PaymentMethod);
        }

        var page = queryParams.Page <= 0 ? 1 : queryParams.Page;
        var pageSize = queryParams.PageSize <= 0 ? 10 : queryParams.PageSize;

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Invoice>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> CheckoutByStoredProcedureAsync(int orderId, int staffId, string paymentMethod, decimal taxAmount, decimal discountAmount, decimal tipAmount)
    {
        var invoiceId = new SqlParameter("@InvoiceId", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Checkout @OrderId, @StaffId, @PaymentMethod, @TaxAmount, @DiscountAmount, @TipAmount, @InvoiceId OUTPUT",
            new SqlParameter("@OrderId", orderId),
            new SqlParameter("@StaffId", staffId),
            new SqlParameter("@PaymentMethod", paymentMethod),
            new SqlParameter("@TaxAmount", taxAmount),
            new SqlParameter("@DiscountAmount", discountAmount),
            new SqlParameter("@TipAmount", tipAmount),
            invoiceId);

        return (int)(invoiceId.Value == DBNull.Value ? 0 : invoiceId.Value);
    }
}
