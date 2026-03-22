using BusinessObjects.DTOs.Orders;
using BusinessObjects.Models;
using Repositories;

namespace Services;

public class OrderService : IOrderService
{
    private static readonly HashSet<string> AllowedKitchenStatuses =
    [
        "pending", "preparing", "served", "cancelled"
    ];

    private readonly IOrderRepository _orderRepository;
    private readonly IGenericRepository<MenuItem> _menuItemRepository;

    public OrderService(IOrderRepository orderRepository, IGenericRepository<MenuItem> menuItemRepository)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetDetailByIdAsync(id);
        return order is null ? null : MapOrder(order);
    }

    public async Task<OrderDto?> GetByReservationIdAsync(int reservationId)
    {
        var order = await _orderRepository.GetByReservationIdAsync(reservationId);
        return order is null ? null : MapOrder(order);
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        var items = await _orderRepository.GetItemsAsync(orderId);
        return items.Select(MapOrderItem);
    }

    public async Task AddItemAsync(int orderId, AddOrderItemRequest request)
    {
        var order = await _orderRepository.GetDetailByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Invoice is not null || string.Equals(order.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Order is closed, cannot add items.");
        }

        var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId)
            ?? throw new InvalidOperationException("Menu item does not exist.");

        if (!menuItem.IsAvailable)
        {
            throw new InvalidOperationException("Menu item is out of stock.");
        }

        await _orderRepository.AddItemByStoredProcedureAsync(orderId, request.MenuItemId, request.Quantity, request.Note);
    }

    public async Task UpdateItemStatusAsync(int orderDetailId, string status)
    {
        if (!AllowedKitchenStatuses.Contains(status))
        {
            throw new InvalidOperationException("Kitchen status must be one of: pending, preparing, served, cancelled.");
        }

        await _orderRepository.UpdateItemStatusByStoredProcedureAsync(orderDetailId, status);
    }

    private static OrderDto MapOrder(Order order) => new()
    {
        Id = order.Id,
        ReservationId = order.ReservationId,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        Note = order.Note,
        InvoiceId = order.Invoice?.Id,
        InvoicePaymentMethod = order.Invoice?.PaymentMethod,
        InvoiceFinalTotal = order.Invoice?.FinalTotal,
        InvoicePaidAt = order.Invoice?.PaidAt,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };

    private static OrderItemDto MapOrderItem(OrderDetail detail) => new()
    {
        Id = detail.Id,
        OrderId = detail.OrderId,
        MenuItemId = detail.MenuItemId,
        MenuItemName = detail.MenuItem?.Name ?? string.Empty,
        Quantity = detail.Quantity,
        UnitPrice = detail.UnitPrice,
        Note = detail.Note,
        ItemStatus = detail.ItemStatus
    };
}
