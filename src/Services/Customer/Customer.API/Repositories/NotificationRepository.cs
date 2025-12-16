using Customer.API.Entities;
using Customer.API.Persistence;
using Customer.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Customer.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly CustomerContext _context;

        public NotificationRepository(CustomerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            string userId, 
            int page = 0, 
            int size = 20, 
            string? type = null, 
            bool? isRead = null)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.Type == type);
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            return await query
                .OrderByDescending(n => n.NotificationDate)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> CreateBulkAsync(IEnumerable<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
            return notifications;
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldNotificationsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysOld);
            var oldNotifications = await _context.Notifications
                .Where(n => n.NotificationDate < cutoffDate && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }
    }
}
