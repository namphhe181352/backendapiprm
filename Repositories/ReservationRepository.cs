using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;
using BusinessObjects.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public async Task<PagedResult<Reservation>> GetFilteredAsync(ReservationQueryParams queryParams)
    {
        var query = _dbSet
            .Include(x => x.Table)
            .Include(x => x.Staff)
            .Include(x => x.Order)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            query = query.Where(x => x.Status == queryParams.Status);
        }

        if (queryParams.TableId.HasValue)
        {
            query = query.Where(x => x.TableId == queryParams.TableId.Value);
        }

        if (queryParams.StaffId.HasValue)
        {
            query = query.Where(x => x.StaffId == queryParams.StaffId.Value);
        }

        if (queryParams.From.HasValue)
        {
            query = query.Where(x => x.CheckInTime >= queryParams.From.Value);
        }

        if (queryParams.To.HasValue)
        {
            query = query.Where(x => x.CheckInTime <= queryParams.To.Value);
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

        return new PagedResult<Reservation>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<Reservation?> GetDetailByIdAsync(int id) =>
        _dbSet
            .Include(x => x.Table)
            .Include(x => x.Staff)
            .Include(x => x.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<int> CreateWithOrderByStoredProcedureAsync(Reservation reservation)
    {
        var tableId = new SqlParameter("@TableId", reservation.TableId);
        var staffId = new SqlParameter("@StaffId", reservation.StaffId);
        var customerName = new SqlParameter("@CustomerName", reservation.CustomerName);
        var customerPhone = new SqlParameter("@CustomerPhone", reservation.CustomerPhone);
        var guestCount = new SqlParameter("@GuestCount", reservation.GuestCount);
        var note = new SqlParameter("@Note", (object?)reservation.Note ?? DBNull.Value);
        var reservationId = new SqlParameter("@ReservationId", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_CreateReservationWithOrder @TableId, @StaffId, @CustomerName, @CustomerPhone, @GuestCount, @Note, @ReservationId OUTPUT",
                tableId, staffId, customerName, customerPhone, guestCount, note, reservationId);

            return (int)(reservationId.Value == DBNull.Value ? 0 : reservationId.Value);
        }
        catch (SqlException)
        {
            // Fallback path in case stored procedure is missing or fails in some environments.
            return await CreateWithOrderWithoutStoredProcedureAsync(reservation);
        }
    }

    private async Task<int> CreateWithOrderWithoutStoredProcedureAsync(Reservation reservation)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var table = await _context.DiningTables.FirstOrDefaultAsync(x => x.Id == reservation.TableId);
        if (table is null)
        {
            throw new KeyNotFoundException("Table does not exist.");
        }

        table.Status = "occupied";

        reservation.Status = string.IsNullOrWhiteSpace(reservation.Status) ? "occupied" : reservation.Status;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            ReservationId = reservation.Id,
            TotalAmount = 0,
            Status = "ordering",
            Note = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return reservation.Id;
    }
}
