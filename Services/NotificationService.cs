using BusinessObjects.DTOs.Notifications;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public class NotificationService : INotificationService
{
    private readonly IUserRepository _userRepository;

    public NotificationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private static readonly List<NotificationDto> Notifications =
    [
        new NotificationDto { Id = 1, UserId = 1, Content = "Welcome to Restaurant backend", IsRead = false, CreatedAt = DateTime.UtcNow }
    ];

    public Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId)
    {
        var data = Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .AsEnumerable();

        return Task.FromResult(data);
    }

    public Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var item = Notifications.FirstOrDefault(x => x.Id == notificationId && x.UserId == userId);
        if (item is null)
        {
            return Task.FromResult(false);
        }

        item.IsRead = true;
        return Task.FromResult(true);
    }

    public async Task CreateOrderIncomingNotificationAsync(int reservationId, int? orderId, string tableName, string customerName, int guestCount)
    {
        var adminUserIds = await _userRepository.Query()
            .Where(x => x.Role == "admin" && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        if (adminUserIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var currentMaxId = Notifications.Count == 0 ? 0 : Notifications.Max(x => x.Id);
        var orderLabel = orderId.HasValue ? $"ORD-{orderId.Value}" : $"RES-{reservationId}";
        var content = $"Đơn mới {orderLabel} | Bàn {tableName} | Khách {customerName} ({guestCount} người).";

        foreach (var userId in adminUserIds)
        {
            currentMaxId++;
            Notifications.Add(new NotificationDto
            {
                Id = currentMaxId,
                UserId = userId,
                Content = content,
                IsRead = false,
                CreatedAt = now,
            });
        }
    }
}
