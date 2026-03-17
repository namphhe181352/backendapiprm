using BusinessObjects.Models;

namespace Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetDetailByIdAsync(int id);
    Task<Order?> GetByReservationIdAsync(int reservationId);
    Task<IEnumerable<OrderDetail>> GetItemsAsync(int orderId);
    Task AddItemByStoredProcedureAsync(int orderId, int menuItemId, int quantity, string? note);
    Task UpdateItemStatusByStoredProcedureAsync(int orderDetailId, string status);
}
