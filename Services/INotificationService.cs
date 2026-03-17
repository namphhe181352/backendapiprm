using BusinessObjects.DTOs.Notifications;

namespace Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
}
