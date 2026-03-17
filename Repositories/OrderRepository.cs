using BusinessObjects.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(Prm393RestaurantContext context) : base(context)
    {
    }

    public Task<Order?> GetDetailByIdAsync(int id) =>
        _dbSet
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.MenuItem)
            .Include(x => x.Invoice)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<Order?> GetByReservationIdAsync(int reservationId) =>
        _dbSet
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.MenuItem)
            .Include(x => x.Invoice)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReservationId == reservationId);

    public async Task<IEnumerable<OrderDetail>> GetItemsAsync(int orderId)
    {
        return await _context.OrderDetails
            .Include(x => x.MenuItem)
            .Where(x => x.OrderId == orderId)
            .OrderByDescending(x => x.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddItemByStoredProcedureAsync(int orderId, int menuItemId, int quantity, string? note)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_AddItemToOrder @OrderId, @MenuItemId, @Quantity, @Note",
            new SqlParameter("@OrderId", orderId),
            new SqlParameter("@MenuItemId", menuItemId),
            new SqlParameter("@Quantity", quantity),
            new SqlParameter("@Note", (object?)note ?? DBNull.Value));
    }

    public async Task UpdateItemStatusByStoredProcedureAsync(int orderDetailId, string status)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateItemStatus @OrderDetailId, @Status",
            new SqlParameter("@OrderDetailId", orderDetailId),
            new SqlParameter("@Status", status));
    }
}
