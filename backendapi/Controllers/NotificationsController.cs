using System.Security.Claims;
using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace backendapi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "admin,staff")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.GetMyNotificationsAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(result));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var success = await _notificationService.MarkAsReadAsync(userId, id);
        if (!success)
        {
            return NotFound(ApiResponse.Fail("Notification not found"));
        }

        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var parsed))
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }

        return parsed;
    }
}
