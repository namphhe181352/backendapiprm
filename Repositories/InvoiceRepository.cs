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
        try
        {
            return await ExecuteCheckoutStoredProcedureV2Async(orderId, staffId, paymentMethod, taxAmount, discountAmount, tipAmount);
        }
        catch (SqlException ex) when (IsCheckoutSignatureMismatch(ex) || IsStoredProcedureMissing(ex))
        {
            try
            {
                // Backward compatibility for old DBs where sp_Checkout only accepts 3 input args.
                return await ExecuteCheckoutStoredProcedureLegacyAsync(orderId, staffId, paymentMethod);
            }
            catch (SqlException)
            {
                // Last-resort fallback keeps checkout usable even when SP version is incompatible.
                return await ExecuteCheckoutWithEntityFallbackAsync(orderId, staffId, paymentMethod, taxAmount, discountAmount, tipAmount);
            }
        }
        catch (SqlException)
        {
            return await ExecuteCheckoutWithEntityFallbackAsync(orderId, staffId, paymentMethod, taxAmount, discountAmount, tipAmount);
        }
    }

    private static bool IsCheckoutSignatureMismatch(SqlException exception) =>
        exception.Number == 8144 ||
        exception.Message.Contains("too many arguments", StringComparison.OrdinalIgnoreCase);

    private static bool IsStoredProcedureMissing(SqlException exception) =>
        exception.Number == 2812 ||
        exception.Message.Contains("could not find stored procedure", StringComparison.OrdinalIgnoreCase);

    private async Task<int> ExecuteCheckoutStoredProcedureV2Async(
        int orderId,
        int staffId,
        string paymentMethod,
        decimal taxAmount,
        decimal discountAmount,
        decimal tipAmount)
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

    private async Task<int> ExecuteCheckoutStoredProcedureLegacyAsync(int orderId, int staffId, string paymentMethod)
    {
        var invoiceId = new SqlParameter("@InvoiceId", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Checkout @OrderId, @StaffId, @PaymentMethod, @InvoiceId OUTPUT",
            new SqlParameter("@OrderId", orderId),
            new SqlParameter("@StaffId", staffId),
            new SqlParameter("@PaymentMethod", paymentMethod),
            invoiceId);

        return (int)(invoiceId.Value == DBNull.Value ? 0 : invoiceId.Value);
    }

    private async Task<int> ExecuteCheckoutWithEntityFallbackAsync(
        int orderId,
        int staffId,
        string paymentMethod,
        decimal taxAmount,
        decimal discountAmount,
        decimal tipAmount)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var order = await _context.Orders
            .Include(x => x.Invoice)
            .Include(x => x.Reservation)
                .ThenInclude(x => x.Table)
            .FirstOrDefaultAsync(x => x.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Invoice is not null)
        {
            return order.Invoice.Id;
        }

        var finalTotal = order.TotalAmount + taxAmount - discountAmount + tipAmount;
        if (finalTotal < 0)
        {
            finalTotal = 0;
        }

        var invoice = new Invoice
        {
            OrderId = orderId,
            StaffId = staffId,
            PaymentMethod = paymentMethod,
            TaxAmount = taxAmount,
            DiscountAmount = discountAmount,
            TipAmount = tipAmount,
            FinalTotal = finalTotal,
            PaidAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);

        order.Status = "completed";

        if (order.Reservation is not null)
        {
            order.Reservation.Status = "completed";
            order.Reservation.CheckOutTime = DateTime.UtcNow;

            if (order.Reservation.Table is not null)
            {
                order.Reservation.Table.Status = "available";
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return invoice.Id;
    }
}
