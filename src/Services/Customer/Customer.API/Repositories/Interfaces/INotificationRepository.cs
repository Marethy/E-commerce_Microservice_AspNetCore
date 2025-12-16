using Customer.API.Entities;

namespace Customer.API.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 0, int size = 20, string? type = null, bool? isRead = null);
        Task<int> GetUnreadCountAsync(string userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> CreateBulkAsync(IEnumerable<Notification> notifications);
        Task UpdateAsync(Notification notification);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(Guid notificationId);
        Task DeleteOldNotificationsAsync(int daysOld = 30);
    }
}
