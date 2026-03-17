using BusinessObjects.DTOs.Orders;

namespace Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto?> GetByReservationIdAsync(int reservationId);
    Task<IEnumerable<OrderItemDto>> GetItemsAsync(int orderId);
    Task AddItemAsync(int orderId, AddOrderItemRequest request);
    Task UpdateItemStatusAsync(int orderDetailId, string status);
}
