using BusinessObjects.DTOs.Notifications;

namespace Services;

public class NotificationService : INotificationService
{
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
}
