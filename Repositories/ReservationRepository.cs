using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reservations;
using BusinessObjects.Models;
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
        await using var transaction = await _context.Database.BeginTransactionAsync();

        reservation.Status = string.IsNullOrWhiteSpace(reservation.Status) ? "occupied" : reservation.Status;
        await _dbSet.AddAsync(reservation);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            ReservationId = reservation.Id,
            TotalAmount = 0,
            Status = "ordering"
        };

        await _context.Set<Order>().AddAsync(order);

        var table = await _context.Set<DiningTable>().FirstOrDefaultAsync(x => x.Id == reservation.TableId)
            ?? throw new InvalidOperationException("Table does not exist.");

        table.Status = "occupied";

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return reservation.Id;
    }
}
