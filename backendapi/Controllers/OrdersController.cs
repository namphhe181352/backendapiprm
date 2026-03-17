using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "admin,staff")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound(ApiResponse<OrderDto>.Fail("Order not found"));
        }

        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpGet("orders/by-reservation/{reservationId:int}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetByReservation(int reservationId)
    {
        var result = await _orderService.GetByReservationIdAsync(reservationId);
        if (result is null)
        {
            return NotFound(ApiResponse<OrderDto>.Fail("Order not found"));
        }

        return Ok(ApiResponse<OrderDto>.Ok(result));
    }

    [HttpGet("orders/{orderId:int}/items")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetItems(int orderId)
    {
        var result = await _orderService.GetItemsAsync(orderId);
        return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Ok(result));
    }

    [HttpPost("orders/{orderId:int}/items")]
    public async Task<ActionResult<ApiResponse>> AddItem(int orderId, [FromBody] AddOrderItemRequest request)
    {
        await _orderService.AddItemAsync(orderId, request);
        return Ok(ApiResponse.Ok("Item added to order"));
    }

    [HttpPatch("order-items/{orderDetailId:int}/status")]
    public async Task<ActionResult<ApiResponse>> UpdateItemStatus(int orderDetailId, [FromBody] UpdateOrderItemStatusRequest request)
    {
        await _orderService.UpdateItemStatusAsync(orderDetailId, request.Status);
        return Ok(ApiResponse.Ok("Order item status updated"));
    }
}
